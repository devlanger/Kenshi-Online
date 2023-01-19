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
using StarterAssets.CombatStates;

public class GameServer : MonoBehaviour, INetEventListener, INetLogger
{
    public static GameServer Instance;
    
    public NetManager _netServer;
    private NetPeer _ourPeer;
    [SerializeField] private GameServerEventsHandler handler;

    private Dictionary<int, Player> _players => handler._players;
    public static bool IsServer => GameRoomNetworkController.Instance == null;

    private static int players = 0;
    
    private static IDockerClient _client;
    private static ConnectionMultiplexer redis;

    private static Dictionary<int, string> tokens = new Dictionary<int, string>();
    private bool started;

    public static Config Configuration = new Config();

    public event Action<NetPeer, Vector3> OnPlayerSpawned;
    public event Action<int, Vector3> OnPlayerPositionUpdate;
    public event Action<int> OnPlayerDespawned;
    
    public class ClaimsDto
    {
        public string Name { get; set; }
    }

    public class Config
    {
        public string jwtSecretKey;
        public string containerName;
        public ushort port;
        public string redis;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void StartServer()
    {
        try
        {
            redis = ConnectionMultiplexer.Connect(Configuration.redis);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        
        NetDebug.Logger = this;
        _netServer = new NetManager(this);
        _netServer.Start(Configuration.port);
        _netServer.BroadcastReceiveEnabled = true;
        _netServer.UpdateTime = 15;
        
        Debug.Log($"Circle ENet Server started on {Configuration.port}");
        started = true;
    }
    
    void Update()
    {
        if(started)
            _netServer.PollEvents();
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
            switch (packetId)
            {
                case PacketId.LoginRequest:
                    var playerId = peer.Id;
                    SendablePacket.Deserialize<LoginRequestPacket>(packetId, reader);
                    SendPacket(playerId, new LoginResponsePacket(playerId));
                    SendPacketToAll(new LoginEventPacket(playerId));
                    foreach (var p in _players)
                    {
                        SendPacket(playerId, new LoginEventPacket(p.Key));
                    }
                    break;
                case PacketId.PositionUpdateRequest:
                    var packet = SendablePacket.Deserialize<PositionUpdateRequestPacket>(packetId, reader);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnPlayerPositionUpdate?.Invoke(packet.playerId, new Vector3(packet.x, packet.y, packet.z));
                    });
                    SendPacketToAll(new PositionUpdatePacket(packet.playerId, packet.x, packet.y, packet.z, packet.rotY));
                    break;
                case PacketId.FsmUpdate:
                    var fsmPacket = SendablePacket.Deserialize<UpdateFsmStatePacket>(packetId, reader);
                    switch (fsmPacket.stateId)
                    {
                        case FSMStateId.attack:
                            if (GameServerEventsHandler.Instance._players.TryGetValue(peer.Id, out var player))
                            {
                                player.playerStateMachine.ChangeState(new AttackState(fsmPacket.attackData));
                            }
                            break;
                    }
                    break;
            }
        }

    private Player GetPlayerById(int playerId)
    {
        return _players[playerId];
    }

    private void SendPacket(int peerId, SendablePacket p)
        {
            GameRoomNetworkController.SendPacket(_netServer.GetPeerById(peerId), p, DeliveryMethod.ReliableSequenced);
        }

        private void SendPacketToAll(SendablePacket p)
        {
            List<NetPeer> list = new List<NetPeer>();
            foreach (var id in _players)
            {
                list.Add(_netServer.GetPeerById(id.Key));
            }
            
            GameRoomNetworkController.SendPacketToMany(list, p, DeliveryMethod.ReliableSequenced);
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

        private void HandleLogout(int playerId)
        {
            if (!_players.ContainsKey(playerId))
                return;

            string username = GetUserClaims(playerId).Name;
            var json = JsonConvert.SerializeObject(new UserRoomStateEventDto
            {
                State = RoomEventState.Left,
                RoomId = GetRoomId(Configuration.port),
                Username = username
            });
            new RabbitMqClient().SendConnectedUser("disconnected_user",json);
            tokens.Remove(playerId);
            UnityMainThreadDispatcher.Instance().Enqueue(() => { OnPlayerDespawned?.Invoke(playerId); });
            SendPacketToAll(new LogoutEventPacket(playerId));
            players--;
            
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
            RoomId = GetRoomId(Configuration.port),
            Username = claims.Name
        });
        client.SendConnectedUser("connected_user",json);
        Debug.Log($"[GAME SERVER] Client connected - ID: {peer.Id} Name: {claims.Name}");
        players++;
        
        UnityMainThreadDispatcher.Instance().Enqueue(() => { OnPlayerSpawned?.Invoke(peer, Vector3.zero); });
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
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (_netServer.ConnectedPeersCount < 100)
        {
            string token = request.Data.GetString();
            if (JwtTokenService.VerifyToken(Configuration.jwtSecretKey, token))   
            {
                var peer = request.Accept();
                tokens[peer.Id] = token;
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
        HandleLogout(peer.Id);

        if (peer == _ourPeer)
            _ourPeer = null;
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
}