using System.Security.Authentication;
using Docker.DotNet;
using Docker.DotNet.Models;
using StackExchange.Redis;
using UDPServer;
using System;
using System.Collections.Generic;
using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using LiteNetLib.Utils;
using Address = ENet.Address;

namespace UDPServer
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
        
        static async Task Main(string[] args)
        {
            Start();
        }

        private static void Start()
        {
            
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
            
            const int maxClients = 100;
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            server.Start(port /* port */);

            Console.WriteLine($"Circle ENet Server started on {port}");

            listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("test");
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
                Console.WriteLine("Client connected - ID: " + peer.Id + ", IP: ");
                //peer.Timeout(32, 1000, 4000);
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
                //Console.WriteLine($"ID: {playerId}, Pos: {x}, {y}");
                SendPacketToAll(new PositionUpdatePacket(playerId, x, y, z));
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

            _players.Remove(playerId);
            SendPacketToAll(new LogoutEventPacket(playerId));
            players--;
            UpdatePlayersAmount(players);
        }
    }
}