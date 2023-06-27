using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Kenshi.Shared;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using Sirenix.OdinInspector;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = Unity.Mathematics.Random;
using ConnectionState = LiteNetLib.ConnectionState;

public class GameRoomNetworkController : MonoBehaviour, INetEventListener
{
    public Player myPlayerFactory;
    public Player otherPlayerFactory;

    public Player LocalPlayer => _myPlayer;
    
    private Player _myPlayer;
    private int _myPlayerId;
    
    private NetManager _netClient;

    private int _skipFrame = 0;
    public Dictionary<int, Player> _players = new Dictionary<int, Player>();

    private NetPeer MyPeer => _netClient == null ? null : _netClient.FirstPeer;
    public float Ping => MyPeer == null ? 0.15f : MyPeer.Ping;

    const int channelID = 0;

    public static ushort Port = 5001;

    public bool Connected = false;
    
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
        _myPlayer.GetComponent<ThirdPersonController>().SetPlayer(_myPlayer);
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
        if (!ConnectionController.Instance)
            return;

        ConnectEnet();
    }

    [Button]
    private void ConnectEnet()
    {
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
        string host = ConnectionController.Instance ? ConnectionController.Host : "127.0.0.1";
        _netClient.Connect(host, Port, ConnectionController.Instance ? ConnectionController.Token : "");

        Debug.Log($"Connecting {host}:{Port}");
    }
    
    private void UpdateENet()
    {
        if (ConnectionController.Instance != null)
            _netClient.PollEvents();
    }

    private void SendPositionUpdate()
    {
        if(_myPlayer == null)
        {
            return;
        }

        var x = _myPlayer.transform.position.x;
        var y = _myPlayer.transform.position.y;
        var z = _myPlayer.transform.position.z;
        var rotY = (byte)(_myPlayer.transform.eulerAngles.y / 5);
        var speed = _myPlayer.animator.GetFloat("Speed");
        
        var packet = new PositionUpdateRequestPacket(_myPlayerId, x, y, z, rotY, speed, _myPlayer.Input.HitPoint);
        SendPacket(MyPeer, packet, DeliveryMethod.Sequenced);
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
        if (packetId != PacketId.PositionUpdateEvent)
        {
            Debug.Log($"Send to many [{packetId}]");
        }

        packet.writer.Put((byte)packetId);
        packet.Serialize(packet.writer);
        foreach (var item in peer)
        {
            if (item == null)
            {
                continue;
            }
            
            item.Send(packet.writer, deliveryMethod);
        }
    }
    
    public static void SendPacketToAll(SendablePacket packet, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        List<NetPeer> list = new List<NetPeer>();
        GameServer.Instance._netServer.GetPeersNonAlloc(list, ConnectionState.Connected);
        SendPacketToMany(list, packet, deliveryMethod);
    }
    
    public static void SendPacket(NetPeer peer, SendablePacket packet, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        if(peer == null) return;
        if (peer.ConnectionState != ConnectionState.Connected) return;


        PacketId packetId = (PacketId)packet.packetId;
        if (packetId != PacketId.PositionUpdateRequest)
        {
            Debug.Log($"Send to {peer.Id} packet [{packetId}]");
        }

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
                _myPlayer.NetworkId = _myPlayerId;
                _players[_myPlayerId] = _myPlayer;
                
                MapLoader.MapToBeLoaded = packet.data.mapId;
                MapLoader.LoadScene();

                _players[_myPlayerId].tps.SetVelocity(Vector3.zero);
                _players[_myPlayerId].GetComponent<Rigidbody>().isKinematic = false;
                _players[_myPlayerId].transform.position = SpawnPointsController.Instance != null ? SpawnPointsController.Instance.GetRandomSpawnPoint() : Vector3.up;
                Connected = true;
                Debug.Log("MyPlayerId: " + packet._playerId);
                Debug.Log("Map: " + packet.data.mapId);
            }
            else if (packetId == PacketId.LoginEvent)
            {   
                var packet = SendablePacket.Deserialize<LoginEventPacket>(packetId, reader);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("OtherPlayerId: " + packet._playerId);
                    var p = SpawnOtherPlayer(packet._playerId);
                    if (p != null)
                    {
                        p.SetStat(StatEventPacket.StatId.username, packet.username);
                    }
                });
            }
            else if (packetId == PacketId.PositionUpdateEvent)
            {
                var packet = SendablePacket.Deserialize<PositionUpdatePacket>(packetId, reader);
                UpdatePosition(packet);
            }
            else if (packetId == PacketId.DeathmatchModeEnd)
            {
                var packet = SendablePacket.Deserialize<DeathmatchModeEndPacket>(packetId, reader);
                FinishDeathmatchMode(packet);
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
            else if (packetId == PacketId.StatEvent)
            {
                var statPacket = SendablePacket.Deserialize<StatEventPacket>(packetId, reader);
                CombatController.Instance.SetPlayerStat(statPacket.data);
            }
            else if (packetId == PacketId.GameEventPacket)
            {
                var gameEventPacket = SendablePacket.Deserialize<GameEventPacket>(packetId, reader);
                switch (gameEventPacket.eventId)
                {
                    case GameEventPacket.GameEventId.player_died:
                        var ui = FindObjectOfType<InGameEventsUI>();
                        if (ui)
                        {
                            ui.AddInGameEventLabel(gameEventPacket.diedData);
                        }
                        if (_players.TryGetValue(gameEventPacket.diedData.playerId, out var p1))
                        {
                            p1.playerStateMachine.ChangeState(new DeadState());
                        }
                        break;
                    case GameEventPacket.GameEventId.player_respawn:
                        if (_players.TryGetValue(gameEventPacket.respawnData.playerId, out var p))
                        {
                            p.transform.position = gameEventPacket.respawnData.respawnPos;
                            p.playerStateMachine.ChangeState(new IdleState());
                        }
                        break;
                    case GameEventPacket.GameEventId.score_changed:
                        // if (_players.TryGetValue(gameEventPacket.respawnData.playerId, out var p2))
                        // {
                        //     p2.transform.position = gameEventPacket.respawnData.respawnPos;
                        //     p2.playerStateMachine.ChangeState(new IdleState());
                        // }
                        break;
                }
            }
            else if (packetId == PacketId.FsmUpdate)
            {
                var fsmPacket = SendablePacket.Deserialize<UpdateFsmStatePacket>(packetId, reader);
                var playerNotNull = _players.TryGetValue(fsmPacket.targetId, out var player);
                switch (fsmPacket.stateId)
                {
                    case FSMStateId.attack:
                        if (playerNotNull && !player.IsLocalPlayer)
                        {
                            player.playerStateMachine.ChangeState(new AttackState(fsmPacket.attackData));
                        }
                        break;
                    case FSMStateId.hit:
                        if (_players.TryGetValue(fsmPacket.hitData.targetId, out var p))
                        {
                            p.playerStateMachine.ChangeState(new HitState(fsmPacket.hitData));
                        }
                        break;
                    case FSMStateId.ability_cast:
                        if (playerNotNull && !player.IsLocalPlayer)
                        {
                            player.playerStateMachine.ChangeState(new AbilityCastState(fsmPacket.abilityData));
                        }
                        break;
                    case FSMStateId.dash:
                        if (playerNotNull && !player.IsLocalPlayer)
                        {
                            player.movementStateMachine.ChangeState(new DashState(fsmPacket.dashData));
                        }
                        break;
                    case FSMStateId.block:
                        if (playerNotNull && !player.IsLocalPlayer)
                        {
                            player.playerStateMachine.ChangeState(new BlockState());
                        }
                        break;
                    case FSMStateId.mana_regen:
                        if (playerNotNull && !player.IsLocalPlayer)
                        {
                            player.playerStateMachine.ChangeState(new ManaRegenState());
                        }
                        break;
                    case FSMStateId.stunned:
                        if (playerNotNull)
                        {
                            player.playerStateMachine.ChangeState(new StunState());
                        }
                        break;
                    case FSMStateId.dead:
                        if (playerNotNull)
                        {
                            player.playerStateMachine.ChangeState(new DeadState());
                        }
                        break;
                }
            }
        }
    }

    private void FinishDeathmatchMode(DeathmatchModeEndPacket packet)
    {
        var ui = FindObjectOfType<DeathmatchCanvas>(true);
        if (ui == null) return;
        
        ui.gameObject.SetActive(true);
    }

    private Player SpawnOtherPlayer(int playerId)
    {
        if (playerId == _myPlayerId)
            return null;
        
        var newPlayer = Instantiate(otherPlayerFactory);
        newPlayer.transform.position = newPlayer.GetComponent<Rigidbody>().position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
        Debug.Log("Spawn other object " + playerId);
        _players[playerId] = newPlayer;
        newPlayer.NetworkId = playerId;
        return _players[playerId];
    }

    private void UpdatePosition(PositionUpdatePacket packet)
    {
        if (packet.playerId == _myPlayerId)
            return;

        if (!_players.ContainsKey(packet.playerId))
        {
            return;
        }

        var p = _players[packet.playerId];
        p.Interpolation.Push(new PositionUpdateSnapshot()
        {
            packet = packet
        });

        if (p.tps != null)
        {
            p.tps.SetSpeed(packet.speed);
        }
        
        p.transform.eulerAngles = new Vector3(0, packet.rotY * 5, 0);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Client connected to server - ID: " + peer.Id);
        SendLogin();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Client disconnected from server");
        LoadingController.Instance.Deactivate();
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

    public static void SendPacketToServer(SendablePacket packet, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        SendPacket(GameRoomNetworkController.Instance.MyPeer, packet, deliveryMethod);
    }
}
