using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ENet;
using LiteNetLib;
using StarterAssets;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using ConnectionState = LiteNetLib.ConnectionState;

public class GameRoomNetworkController : MonoBehaviour, INetEventListener
{
    public GameObject myPlayerFactory;
    public GameObject otherPlayerFactory;

    private GameObject _myPlayer;
    private uint _myPlayerId;
    
    private NetManager _netClient;

    private int _skipFrame = 0;
    private Dictionary<uint, GameObject> _players = new Dictionary<uint, GameObject>();

    const int channelID = 0;

    public static ushort Port = 5001;
    
    void Start ()
    {
        Application.runInBackground = true;
        InitENet();
        _myPlayer = myPlayerFactory;     //Instantiate(myPlayerFactory);
        FindObjectOfType<ThirdPersonController>().SetPlayer(_myPlayer.transform);
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
        _netClient.DisconnectAll();
    }

    private void InitENet()
    {
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
        _netClient.Connect(ConnectionController.Host, Port, "test");
        
        Debug.Log($"Connecting {ConnectionController.Host}:{Port}");
    }

    private void UpdateENet()
    {
        _netClient.PollEvents();

        var peer = _netClient.FirstPeer;
        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
        }
        else
        {
            _netClient.SendBroadcast(new byte[] {1}, 5000);
        }
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

        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.PositionUpdateRequest, _myPlayerId, x, y, z);
        _netClient.FirstPeer.Send(buffer, DeliveryMethod.Unreliable);
    }

    private void SendLogin()
    {
        Debug.Log("SendLogin");
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.LoginRequest, 0);
        _netClient.FirstPeer.Send(buffer, DeliveryMethod.ReliableOrdered);
    }

    private void ParsePacket(NetPacketReader reader)
    {
        var packetId = (PacketId)reader.GetByte();

        //Debug.Log("ParsePacket received: " + packetId);

        if (packetId == PacketId.LoginResponse)
        {
            _myPlayerId = reader.GetUInt();
            Debug.Log("MyPlayerId: " + _myPlayerId);
        }
        else if (packetId == PacketId.LoginEvent)
        {
            var playerId = reader.GetUInt();
            Debug.Log("OtherPlayerId: " + playerId);
            SpawnOtherPlayer(playerId);
        }
        else if (packetId == PacketId.PositionUpdateEvent)
        {
            var playerId = reader.GetUInt();
            var x = reader.GetFloat();
            var y = reader.GetFloat();
            var z = reader.GetFloat();
            UpdatePosition(playerId, x, y, z);
        }
        else if (packetId == PacketId.LogoutEvent)
        {
            var playerId = reader.GetUInt();
            if (_players.ContainsKey(playerId))
            {
                Destroy(_players[playerId]);
                _players.Remove(playerId);
            }
        }
    }

    private void SpawnOtherPlayer(uint playerId)
    {
        if (playerId == _myPlayerId)
            return;
        var newPlayer = Instantiate(otherPlayerFactory);
        newPlayer.transform.position = newPlayer.GetComponent<Rigidbody>().position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
        Debug.Log("Spawn other object " + playerId);
        _players[playerId] = newPlayer;
    }

    private void UpdatePosition(uint playerId, float x, float y, float z)
    {
        if (playerId == _myPlayerId)
            return;

        Debug.Log($"UpdatePosition {x} {y} {z}");
        _players[playerId].transform.position = new Vector3(x, y, z);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Client connected to server - ID: " + peer.Id);
        SendLogin();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Client disconnected from server");

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
