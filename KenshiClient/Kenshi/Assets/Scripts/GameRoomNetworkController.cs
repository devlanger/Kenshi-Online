using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ENet;
using Kenshi.Shared;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = Unity.Mathematics.Random;
using ConnectionState = LiteNetLib.ConnectionState;

public class GameRoomNetworkController : MonoBehaviour, INetEventListener
{
    public Player myPlayerFactory;
    public Player otherPlayerFactory;

    private Player _myPlayer;
    private int _myPlayerId;
    
    private NetManager _netClient;

    private int _skipFrame = 0;
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();

    private NetPeer MyPeer => _netClient.FirstPeer;
    
    const int channelID = 0;

    public static ushort Port = 5001;

    public static GameRoomNetworkController Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start ()
    {
        Application.runInBackground = true;
        InitENet();
        _myPlayer = myPlayerFactory;     //Instantiate(myPlayerFactory);
        FindObjectOfType<ThirdPersonController>().SetPlayer(_myPlayer);
    }

	void Update ()
    {
    }

    void FixedUpdate()
    {
        UpdateENet();

        if (++_skipFrame < 3)
            return;

        SendPositionUpdate();
        _skipFrame = 0;
    }

    void OnDestroy()
    {
        _netClient?.DisconnectAll();
    }

    private void InitENet()
    {
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
        string host = ConnectionController.Instance ? ConnectionController.Host : "127.0.0.1";
        _netClient.Connect(host, Port, ConnectionController.Token);
        
        Debug.Log($"Connecting {host}:{Port}");
    }

    private void UpdateENet()
    {
        _netClient.PollEvents();
    }

    private void SendPositionUpdate()
    {
        var x = _myPlayer.transform.position.x;
        var y = _myPlayer.transform.position.y;
        var z = _myPlayer.transform.position.z;
        var rotY = (byte)(_myPlayer.transform.eulerAngles.y / 5);
        
        var packet = new PositionUpdateRequestPacket(_myPlayerId, x, y, z, rotY);
        SendPacket(MyPeer, packet, DeliveryMethod.Unreliable);
    }
    
    private void SendLogin()
    {
        Debug.Log("SendLogin");
        SendPacket(MyPeer, new LoginRequestPacket()
        {
            _playerId = 0
        }, DeliveryMethod.ReliableOrdered);
    }

    public Dictionary<PacketId, Type> packetTypes = new Dictionary<PacketId, Type>
    {
        { PacketId.LoginEvent, typeof(LoginEventPacket) },
        { PacketId.LogoutEvent, typeof(LogoutEventPacket) },
        { PacketId.LoginResponse, typeof(LoginResponsePacket) },
        { PacketId.LoginRequest, typeof(LoginRequestPacket) },
        { PacketId.PositionUpdateEvent, typeof(PositionUpdatePacket) },
        { PacketId.PositionUpdateRequest, typeof(PositionUpdateRequestPacket) },
    };

    public static void SendPacketToMany(IEnumerable<NetPeer> peer, SendablePacket packet, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        PacketId packetId = (PacketId)packet.packetId;
        Debug.Log($"Send to many [{packetId}]");
        packet.writer.Put((byte)packetId);
        packet.Serialize(packet.writer);
        foreach (var item in peer)
        {
            item.Send(packet.writer, deliveryMethod);
        }
    }
    
    public static void SendPacket(NetPeer peer, SendablePacket packet, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        PacketId packetId = (PacketId)packet.packetId;
        Debug.Log($"Send to {peer.Id} packet [{packetId}]");
        packet.writer.Put((byte)packetId);
        packet.Serialize(packet.writer);
        peer.Send(packet.writer, deliveryMethod);
    }
    
    private void ParsePacket(NetPacketReader reader)
    {
        while (!reader.EndOfData)
        {
            PacketId packetId = (PacketId)reader.GetByte();

            if (packetId == PacketId.LoginResponse)
            {
                var packet = SendablePacket.Deserialize<LoginResponsePacket>(packetId, reader);
                _myPlayerId = packet._playerId;
                Debug.Log("MyPlayerId: " + packet._playerId);
            }
            else if (packetId == PacketId.LoginEvent)
            {
                var packet = SendablePacket.Deserialize<LoginEventPacket>(packetId, reader);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("OtherPlayerId: " + packet._playerId);
                    SpawnOtherPlayer(packet._playerId);
                });
            }
            else if (packetId == PacketId.PositionUpdateEvent)
            {
                var packet = SendablePacket.Deserialize<PositionUpdatePacket>(packetId, reader);

                UnityMainThreadDispatcher.Instance().Enqueue(() => { UpdatePosition(packet); });
            }
            else if (packetId == PacketId.LogoutEvent)
            {
                var packet = SendablePacket.Deserialize<LogoutEventPacket>(packetId, reader);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log($"logout user [{packet.PlayerId}]");
                    if (_players.ContainsKey(packet.PlayerId))
                    {
                        Destroy(_players[packet.PlayerId].gameObject);
                        _players.Remove(packet.PlayerId);
                    }
                });
            }
        }
    }

    private void SpawnOtherPlayer(int playerId)
    {
        if (playerId == _myPlayerId)
            return;
        
        var newPlayer = Instantiate(otherPlayerFactory);
        newPlayer.transform.position = newPlayer.GetComponent<Rigidbody>().position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
        Debug.Log("Spawn other object " + playerId);
        _players[playerId] = newPlayer;
    }

    private void UpdatePosition(PositionUpdatePacket packet)
    {
        if (packet.playerId == _myPlayerId)
            return;

        if (!_players.ContainsKey(packet.playerId))
        {
            return;
        }
        
        _players[packet.playerId].Interpolation.Push(new PositionUpdateSnapshot()
        {
            packet = packet
        });
        //_players[packet.playerId].transform.position = new Vector3(packet.x, packet.y, packet.z);
        _players[packet.playerId].transform.eulerAngles = new Vector3(0, packet.rotY * 5, 0);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Client connected to server - ID: " + peer.Id);
        SendLogin();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Client disconnected from server");
        SceneManager.LoadScene(0);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        ParsePacket(reader);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
    }
}
