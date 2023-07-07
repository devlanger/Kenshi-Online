using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Packets.GameServer;
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

    public List<Player> GetAllPlayers() => _players.Values.ToList();

    public event Action<Player> OnPlayerJoined;
    
    private void Awake()
    {
        Instance = this;
        server.OnPlayerSpawned += ServerOnOnPlayerSpawned;
        server.OnPlayerDespawned += ServerOnOnPlayerDespawned;
        server.OnPlayerPositionUpdate += ServerOnPlayerPositionUpdate;
    }

    private void ServerOnPlayerPositionUpdate(PositionUpdateRequestPacket packet)
    {
        if (!_players.ContainsKey(packet.playerId))
        {
            return;
        }

        _players[packet.playerId].transform.position = new Vector3(packet.x, packet.y, packet.z);
        _players[packet.playerId].transform.rotation = Quaternion.Euler(0, ((int)packet.rotY * 5), 0);
        _players[packet.playerId].Input.HitPoint = packet._inputHitPoint;
    }

    private int lastBotId = 1000;
    
    private void ServerOnOnPlayerSpawned(NetPeer arg1, GameServer.ClaimsDto claims,Vector3 arg2)
    {
        if (_players.ContainsKey(arg1.Id))
        {
            return;
        }

        var inst = Instantiate(playerObject, arg2, Quaternion.identity);
        inst.stats[StatEventPacket.StatId.username] = claims.Name;
        AddPlayerToNetwork(arg1.Id, inst);
        _players[arg1.Id].peer = arg1;
    }
    
    public void AddBotToNetwork(Player p)
    {
        p.stats[StatEventPacket.StatId.username] = $"Bot-{UnityEngine.Random.Range(1, 9999)}";
        AddPlayerToNetwork(lastBotId++, p);
        bots[p.NetworkId] = p;
        p.IsBot = true;
    }
    
    private void AddPlayerToNetwork(int id, Player p)
    {
        _players[id] = p;
        _players[id].NetworkId = id;
        OnPlayerJoined?.Invoke(p);
    }

    private void ServerOnOnPlayerDespawned(int obj)
    {
        Destroy(_players[obj].gameObject);
        _players.Remove(obj);
    }
}
