using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class GameServerEventsHandler : MonoBehaviour
{
    public static GameServerEventsHandler Instance;
    public GameServer server;
    
    [SerializeField] private Player playerObject;

    public Dictionary<int, Player> _players = new Dictionary<int, Player>();

    private void Awake()
    {
        Instance = this;
        server.OnPlayerSpawned += ServerOnOnPlayerSpawned;
        server.OnPlayerDespawned += ServerOnOnPlayerDespawned;
        server.OnPlayerPositionUpdate += ServerOnPlayerPositionUpdate;
    }

    private void ServerOnPlayerPositionUpdate(int arg1, Vector3 arg2)
    {
        if (!_players.ContainsKey(arg1))
        {
            return;
        }

        _players[arg1].transform.position = arg2;
    }

    private void ServerOnOnPlayerSpawned(NetPeer arg1, Vector3 arg2)
    {
        if (_players.ContainsKey(arg1.Id))
        {
            return;
        }

        _players[arg1.Id] = Instantiate(playerObject, arg2, Quaternion.identity);
        _players[arg1.Id].peer = arg1;
        _players[arg1.Id].NetworkId = arg1.Id;
    }

    private void ServerOnOnPlayerDespawned(int obj)
    {
        Destroy(_players[obj].gameObject);
        _players.Remove(obj);
    }
}
