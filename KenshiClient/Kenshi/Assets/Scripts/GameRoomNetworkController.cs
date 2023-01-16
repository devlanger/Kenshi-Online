using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ENet;
using Kenshi.Shared;
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

    const int channelID = 0;

    public static ushort Port = 5001;
    
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

    enum PacketId : byte
    {
        LoginRequest = 1,
        LoginResponse = 2,
        LoginEvent = 3,
        PositionUpdateRequest = 4,
        PositionUpdateEvent = 5,
        LogoutEvent = 6
    }

    private void SendPositionUpdate()
    {
        var x = _myPlayer.transform.position.x;
        var y = _myPlayer.transform.position.y;
        var z = _myPlayer.transform.position.z;
        var rotY = (byte)(_myPlayer.transform.eulerAngles.y / 5);

        var packet = new PositionUpdateRequestPacket(_myPlayerId, x, y, z, rotY);
        var protocol = new Protocol();
        
        var buffer = protocol.Serialize(packet);
        _netClient.FirstPeer.Send(buffer, DeliveryMethod.Unreliable);
    }

    private void SendLogin()
    {
        Debug.Log("SendLogin");
        var protocol = new Protocol();
        var buffer = protocol.Serialize(new LoginRequestPacket()
        {
            _playerId = 0
        });
        _netClient.FirstPeer.Send(buffer, DeliveryMethod.ReliableOrdered);
    }

    private void ParsePacket(NetPacketReader reader)
    {
        var packetId = (PacketId)reader.GetByte();

        if (packetId == PacketId.LoginResponse)
        {
            _myPlayerId = reader.GetInt();
            Debug.Log("MyPlayerId: " + _myPlayerId);
        }
        else if (packetId == PacketId.LoginEvent)
        {
            var playerId = reader.GetInt();
            Debug.Log("OtherPlayerId: " + playerId);
            SpawnOtherPlayer(playerId);
        }
        else if (packetId == PacketId.PositionUpdateEvent)
        {
            var playerId = reader.GetInt();
            var x = reader.GetFloat();
            var y = reader.GetFloat();
            var z = reader.GetFloat();
            var rotY  = reader.GetByte();
            
            UpdatePosition(playerId, x, y, z, rotY);
        }
        else if (packetId == PacketId.LogoutEvent)
        {
            var playerId = reader.GetInt();
            Debug.Log($"logout user [{playerId}]");
            if (_players.ContainsKey(playerId))
            {
                Destroy(_players[playerId]);
                _players.Remove(playerId);
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

    private void UpdatePosition(int playerId, float x, float y, float z, byte rotY)
    {
        if (playerId == _myPlayerId)
            return;

        if (!_players.ContainsKey(playerId))
        {
            return;
        }
        
        _players[playerId].transform.position = new Vector3(x, y, z);
        _players[playerId].transform.eulerAngles = new Vector3(0, rotY * 5, 0);
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
