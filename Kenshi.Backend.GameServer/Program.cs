using System.Security.Authentication;
using Docker.DotNet;
using Docker.DotNet.Models;
using StackExchange.Redis;
using UDPServer;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using Kenshi.Backend.Shared.Models;
using Kenshi.Shared;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using Address = ENet.Address;

namespace Kenshi.Backend.GameServer
{
    class Program
    {
        struct Position
        {
            public float x;
            public float y;
            public float z;
        }

        private static Dictionary<int, Position> _players = new Dictionary<int, Position>();

        private static string containerName = "test";
        private static int players = 0;
        private static IDockerClient _client;

        private static ConnectionMultiplexer redis;
        
        private static bool UpdatePlayersAmount(int i)
        {
            try
            {
                var db = redis.GetDatabase();
                db.StringSet($"{containerName}_players", i.ToString());
                Console.WriteLine("Players Count: " + db.StringGet($"{containerName}_players"));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Redis connection error: " + e);
                return false;
            }
        }

        private static EventBasedNetListener listener;
        private static NetManager server;
        private static ushort port;
        private static Dictionary<int, string> tokens = new Dictionary<int, string>();

        public class ClaimsDto
        {
            public string Name { get; set; }
        }
        
        public static ClaimsDto GetUserClaims(int playerId)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens[playerId]);
            return new ClaimsDto()
            {
                Name = jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value
            };
        }
        
        static async Task Main(string[] args)
        {
            Start();
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
        
        private static void Start()
        {
            string jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

            containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "test";
            port = 5001;
            try
            {
                redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            port = ushort.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001");
            if (UpdatePlayersAmount(0))
            {
                Console.WriteLine("Successfully connected to redis and initialized server parameters.");
            }
            
            const int maxClients = 16;
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            server.Start(port /* port */);

            Console.WriteLine($"Circle ENet Server started on {port}");

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < maxClients)
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
            };

            listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
            {
                HandlePacket(peer, reader);
            };

            listener.PeerDisconnectedEvent += (peer, info) =>
            {
                HandleLogout(peer.Id);
            };
        
            listener.PeerConnectedEvent += peer =>
            {
                var claims = GetUserClaims(peer.Id);
                RabbitMqClient client = new RabbitMqClient();
                var json = JsonConvert.SerializeObject(new UserRoomStateEventDto
                {
                    State = RoomEventState.Joined,
                    RoomId = GetRoomId(port),
                    Username = claims.Name
                });
                client.SendConnectedUser("connected_user",json);
                Console.WriteLine($"[GAME SERVER] Client connected - ID: {peer.Id} Name: {claims.Name}");
                players++;
                UpdatePlayersAmount(players);
            };
            
            while (true)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
        }
        
        static void HandlePacket(NetPeer peer, NetPacketReader reader)
        {
            var packetId = (PacketId)reader.GetByte();

            if (packetId != PacketId.PositionUpdateRequest)
                Console.WriteLine($"HandlePacket received: {packetId}");

            if (packetId == PacketId.LoginRequest)
            {
                var playerId = peer.Id;
                SendPacket(playerId, new LoginResponsePacket(playerId));
                SendPacketToAll(new LoginEventPacket(playerId));
                foreach (var p in _players)
                {
                    SendPacket(playerId, new LoginEventPacket(p.Key));
                }
                _players.Add(playerId, new Position { x = 0.0f, y = 0.0f, z = 0.0f });
            }
            else if (packetId == PacketId.PositionUpdateRequest)
            {
                var playerId = reader.GetUInt();
                var x = reader.GetFloat();
                var y = reader.GetFloat();
                var z = reader.GetFloat();
                var rotY = reader.GetByte();
                //Console.WriteLine($"ID: {playerId}, Pos: {x}, {y}");
                SendPacketToAll(new PositionUpdatePacket(playerId, x, y, z, rotY));
            }
        }

        static void SendPacket(int peerId, SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            
            server.GetPeerById(peerId).Send(buffer, DeliveryMethod.ReliableSequenced);
        }

        static void SendPacketToAll(SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            foreach (var id in _players)
            {
                server.GetPeerById(id.Key).Send(buffer, DeliveryMethod.ReliableSequenced);
            }
        }

        static void HandleLogout(int playerId)
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
            
            Console.WriteLine($"User has disconnected {username}");
        }
    }
}