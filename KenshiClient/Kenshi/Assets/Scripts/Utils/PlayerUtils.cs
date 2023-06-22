using System.Collections.Generic;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;

namespace Kenshi.Utils
{
    public class PlayerUtils
    {
        public class HitResponse
        {
            public bool blocked;
            public bool dead;
            public bool success = false;
        }
        
        public static HitResponse HitSingleTarget(AttackState.DamageData data)
        {
            var hitTarget = data.hitTarget;
            var attacker = data.attacker;
            var damage = data.damage;
            var response = new HitResponse();

            var hitData = new HitState.Data
            {
                attackerId = attacker.NetworkId,
                targetId = hitTarget.NetworkId,
                hitPos = hitTarget.transform.position,
                direction = data.direction,
                duration = GetHitDuration(data),
                hitType = data.hitType
            };

            switch (hitTarget.playerStateMachine.CurrentState.Id)
            {
                case FSMStateId.block:
                    response.success = false;
                    response.blocked = true;
                    break;
                case FSMStateId.dead:
                    response.success = false;
                    response.dead = true;
                    break;
                default:
                    if (hitTarget.GetStat(StatEventPacket.StatId.health, out ushort health) && health > 0)
                    {
                        health = (ushort)Mathf.Max(0, health - damage);
                        if (health <= 0)
                        {
                            health = 0;
                            GameRoomNetworkController.SendPacketToAll(new GameEventPacket(new GameEventPacket.PlayerDied
                            {
                                playerId = hitTarget.NetworkId,
                                targetName = (string)hitTarget.stats[StatEventPacket.StatId.username],
                                attackerName = (string)attacker.stats[StatEventPacket.StatId.username],
                                dt = GameEventPacket.PlayerDied.DeathType.melee
                            }), DeliveryMethod.ReliableOrdered);

                            response.dead = true;
                        }

                        CombatController.Instance.SetPlayerStat(new StatEventPacket.Data
                        {
                            statId = StatEventPacket.StatId.health,
                            value = health,
                            maxValue = (ushort)100,
                            playerId = hitTarget.NetworkId
                        });

                        response.success = true;
                    }
                    break;
            }

            if (response.success)
            {
                switch (data.hitType)
                {
                    case AttackState.DamageData.HitType.stun:
                        GameRoomNetworkController.SendPacketToAll(
                            new UpdateFsmStatePacket(hitData.targetId, hitData),
                            DeliveryMethod.ReliableOrdered);

                        hitTarget.playerStateMachine.ChangeState(new HitState(hitData));
                        hitTarget.playerStateMachine.ChangeState(new StunState());
                        break;
                    default:
                        GameRoomNetworkController.SendPacketToAll(
                            new UpdateFsmStatePacket(hitData.targetId, hitData),
                            DeliveryMethod.ReliableOrdered);

                        hitTarget.playerStateMachine.ChangeState(new HitState(hitData));
                        break;
                }

                if (response.dead && hitTarget.playerStateMachine.CurrentState.Id != FSMStateId.dead)
                {
                    hitTarget.playerStateMachine.ChangeState(new DeadState());
                    CombatController.Instance.DeadPlayer(hitTarget);
                }
            }

            return response;
        }

        private static float GetHitDuration(AttackState.DamageData data)
        {
            switch (data.hitType)
            {
                case AttackState.DamageData.HitType.very_light:
                    return 0.4f;
                case AttackState.DamageData.HitType.light:
                    return 0.8f;
                case AttackState.DamageData.HitType.heavy:
                    return 1.4f;
            }
            return 1f;
        }

        public static List<Player> GetPlayersAtPos(Vector3 pos, float distance, Player ignoredPlayer = null)
        {
            var result = new List<Player>();
            var players = Physics.OverlapSphere(pos, distance);
            foreach (var p in players)
            {
                if (p.TryGetComponent<Player>(out var comp))
                {
                    if (ignoredPlayer != null && comp.NetworkId == ignoredPlayer.NetworkId)
                    {
                        continue;
                    }
                    
                    result.Add(comp);
                }
            }

            return result;
        }
    }
}