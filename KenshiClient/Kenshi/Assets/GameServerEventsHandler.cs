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
    public Dictionary<int, Player> bots = new Dictionary<int, Player>();

    private void Awake()
    {
        Instance = this;
        server.OnPlayerSpawned += ServerOnOnPlayerSpawned;
        server.OnPlayerDespawned += ServerOnOnPlayerDespawned;
        server.OnPlayerPositionUpdate += ServerOnPlayerPositionUpdate;

        foreach (var p in FindObjectsOfType<Player>())
        {
            AddBotToNetwork(p);
        }
    }

    private void ServerOnPlayerPositionUpdate(int arg1, Vector3 arg2)
    {
        if (!_players.ContainsKey(arg1))
        {
            return;
        }

        _players[arg1].transform.position = arg2;
    }

    private int lastBotId = 1000;
    
    private void ServerOnOnPlayerSpawned(NetPeer arg1, Vector3 arg2)
    {
        if (_players.ContainsKey(arg1.Id))
        {
            return;
        }

        var inst = Instantiate(playerObject, arg2, Quaternion.identity);
        AddPlayerToNetwork(arg1.Id, inst);
        _players[arg1.Id].peer = arg1;
    }
    
    private void AddBotToNetwork(Player p)
    {
        AddPlayerToNetwork(lastBotId++, p);
        bots[p.NetworkId] = p;
    }
    
    private void AddPlayerToNetwork(int id, Player p)
    {
        _players[id] = p;
        _players[id].NetworkId = id;
    }

    private void ServerOnOnPlayerDespawned(int obj)
    {
        Destroy(_players[obj].gameObject);
        _players.Remove(obj);
    }
}
