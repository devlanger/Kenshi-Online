using System;
using System.Collections;
using Kenshi.Shared.Packets.GameServer;
using Kenshi.Utils;
using LiteNetLib;
using StarterAssets.CombatStates;
using UnityEngine;

namespace StarterAssets
{
    public class CombatController : MonoBehaviour
    {
        public static CombatController Instance;

        public event Action<StatEventPacket.Data> OnStatsChanged;
        public event Action<Player, Player> OnPlayerDeath;
        
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

        public void HitSingleTarget(AttackState.DamageData damageData)
        {
            var response = PlayerUtils.HitSingleTarget(damageData);

            if (response.success && response.dead)
            {
                CombatController.Instance.StartCoroutine(RespawnPlayer(damageData.hitTarget));
            }
        }
        
        private IEnumerator RespawnPlayer(Player deadPlayer)
        {
            yield return new WaitForSeconds(5);

            var pos = SpawnPointsController.Instance.GetRandomSpawnPoint();
            
            GameRoomNetworkController.SendPacketToAll(new GameEventPacket(new GameEventPacket.PlayerRespawn
            {
                playerId = deadPlayer.NetworkId,
                respawnPos = pos
            }), DeliveryMethod.ReliableOrdered);

            deadPlayer.transform.position = pos;
            deadPlayer.playerStateMachine.ChangeState(new IdleState());
            
            CombatController.Instance.SetPlayerStat(new StatEventPacket.Data
            {
                statId = StatEventPacket.StatId.health,
                value = (ushort)100,
                maxValue = (ushort)100,
                playerId = deadPlayer.NetworkId
            });
        }

        public void DeadPlayer(Player attacker, Player hitTarget)
        {
            OnPlayerDeath?.Invoke(attacker, hitTarget);
        }
    }
}