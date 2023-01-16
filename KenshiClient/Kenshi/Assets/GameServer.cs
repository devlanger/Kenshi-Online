using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using Docker.DotNet;
using Kenshi.Backend.Shared.Models;
using Kenshi.Shared;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;
using UnityEngine.UIElements;

public class GameServer : MonoBehaviour, INetEventListener, INetLogger
{
    private NetManager _netServer;
    private NetPeer _ourPeer;
    private NetDataWriter _dataWriter;

    private static Dictionary<int, Vector3> _players = new Dictionary<int, Vector3>();

    private static string containerName = "test";
    private static int players = 0;
    
    private static IDockerClient _client;
    private static ConnectionMultiplexer redis;

    private static ushort port;
    private static Dictionary<int, string> tokens = new Dictionary<int, string>();
    private string jwtSecretKey;
    
    public class ClaimsDto
    {
        public string Name { get; set; }
    }
    
    void Start()
    {
        jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

        containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "test";
        port = 5001;
        try
        {
            redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        port = ushort.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001");
        if (UpdatePlayersAmount(0))
        {
            Debug.Log("Successfully connected to redis and initialized server parameters.");
        }
        
        NetDebug.Logger = this;
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this);
        _netServer.Start(port);
        _netServer.BroadcastReceiveEnabled = true;
        _netServer.UpdateTime = 15;
        
        Debug.Log($"Circle ENet Server started on {port}");
    }

    private static bool UpdatePlayersAmount(int i)
    {
        try
        {
            var db = redis.GetDatabase();
            db.StringSet($"{containerName}_players", i.ToString());
            Debug.Log("Players Count: " + db.StringGet($"{containerName}_players"));
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Redis connection error: " + e);
            return false;
        }
    }
    
    void Update()
    {
        _netServer.PollEvents();
    }

    void FixedUpdate()
    {
        if (_ourPeer != null)
        {
            // _serverBall.transform.Translate(1f * Time.fixedDeltaTime, 0f, 0f);
            // _dataWriter.Reset();
            // _dataWriter.Put(_serverBall.transform.position.x);
            // _ourPeer.Send(_dataWriter, DeliveryMethod.Sequenced);
        }
    }

    void OnDestroy()
    {
        NetDebug.Logger = null;
        if (_netServer != null)
            _netServer.Stop();
    }
    
    private void HandlePacket(NetPeer peer, NetPacketReader reader)
        {
            var packetId = (PacketId)reader.GetByte();

            if (packetId != PacketId.PositionUpdateRequest)
                Debug.Log($"HandlePacket received: {packetId}");

            if (packetId == PacketId.LoginRequest)
            {
                var playerId = peer.Id;
                SendPacket(playerId, new LoginResponsePacket(playerId));
                SendPacketToAll(new LoginEventPacket(playerId));
                foreach (var p in _players)
                {
                    SendPacket(playerId, new LoginEventPacket(p.Key));
                }
                _players.Add(playerId, new Vector3() { x = 0.0f, y = 0.0f, z = 0.0f });
            }
            else if (packetId == PacketId.PositionUpdateRequest)
            {
                var playerId = reader.GetInt();
                var x = reader.GetFloat();
                var y = reader.GetFloat();
                var z = reader.GetFloat();
                var rotY = reader.GetByte();
                SendPacketToAll(new PositionUpdatePacket(playerId, x, y, z, rotY));
            }
        }

        private void SendPacket(int peerId, SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            
            _netServer.GetPeerById(peerId).Send(buffer, DeliveryMethod.ReliableSequenced);
        }

        private void SendPacketToAll(SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            foreach (var id in _players)
            {
                _netServer.GetPeerById(id.Key).Send(buffer, DeliveryMethod.ReliableSequenced);
            }
        }

        public static ClaimsDto GetUserClaims(int playerId)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens[playerId]);
            return new ClaimsDto()
            {
                Name = jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value
            };
        }
        
        private static string GetRoomId(int port) => $"gameroom-{port}";

        public static void AddPlayer(string player)
        {
            redis.GetDatabase().ListLeftPush(GetRoomId(port), player);
        }
        
        public static void RemovePlayer(string player)
        {
            redis.GetDatabase().ListRemove(GetRoomId(port), player);
        }

        public static List<string> GetPlayers(string room)
        {
            return redis.GetDatabase().ListRange(GetRoomId(port)).Select(x => x.ToString()).ToList();
        }


        private void HandleLogout(int playerId)
        {
            if (!_players.ContainsKey(playerId))
                return;

            string username = GetUserClaims(playerId).Name;
            var json = JsonConvert.SerializeObject(new UserRoomStateEventDto
            {
                State = RoomEventState.Left,
                RoomId = GetRoomId(port),
                Username = username
            });
            new RabbitMqClient().SendConnectedUser("disconnected_user",json);
            RemovePlayer(username);
            tokens.Remove(playerId);
            _players.Remove(playerId);
            SendPacketToAll(new LogoutEventPacket(playerId));
            players--;
            UpdatePlayersAmount(players);
            
            Debug.Log($"User has disconnected {username}");
        }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[SERVER] We have new peer " + peer.EndPoint);
        
        var claims = GetUserClaims(peer.Id);
        RabbitMqClient client = new RabbitMqClient();
        var json = JsonConvert.SerializeObject(new UserRoomStateEventDto
        {
            State = RoomEventState.Joined,
            RoomId = GetRoomId(port),
            Username = claims.Name
        });
        client.SendConnectedUser("connected_user",json);
        Debug.Log($"[GAME SERVER] Client connected - ID: {peer.Id} Name: {claims.Name}");
        players++;
        UpdatePlayersAmount(players);
        
        _ourPeer = peer;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {        
        HandlePacket(peer, reader);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
        // if (messageType == UnconnectedMessageType.Broadcast)
        // {
        //     Debug.Log("[SERVER] Received discovery request. Send discovery response");
        //     NetDataWriter resp = new NetDataWriter();
        //     resp.Put(1);
        //     _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
        // }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (_netServer.ConnectedPeersCount < 100)
        {
            string token = request.Data.GetString();
            if (JwtTokenService.VerifyToken(jwtSecretKey, token))   
            {
                var peer = request.Accept();
                tokens.Add(peer.Id, token);
                AddPlayer(GetUserClaims(peer.Id).Name);
            }
            else
            {
                request.Reject();
            }
        }
        else
            request.Reject();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        //TODO: fix disconnection issue (peer.id shuffling)
        HandleLogout(peer.Id);

        if (peer == _ourPeer)
            _ourPeer = null;
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
}