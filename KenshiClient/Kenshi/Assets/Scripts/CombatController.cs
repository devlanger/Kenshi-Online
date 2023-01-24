using System;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets
{
    public class CombatController : MonoBehaviour
    {
        public static CombatController Instance;

        public event Action<StatEventPacket.Data> OnStatsChanged;
        
        private void Awake()
        {
            Instance = this;
        }

        public void SetPlayerStat(StatEventPacket.Data data)
        {
            Player p;

            if(GameServer.IsServer)
            {
                GameServerEventsHandler.Instance._players.TryGetValue(data.playerId, out p);
            }
            else
            {
                GameRoomNetworkController.Instance._players.TryGetValue(data.playerId, out p);
            }
            
            if(p != null)
            {
                p.stats[data.statId] = data.value;
                OnStatsChanged?.Invoke(data);
            
                if (GameServer.IsServer)
                {
                    GameRoomNetworkController.SendPacketToAll(new StatEventPacket(data), DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}