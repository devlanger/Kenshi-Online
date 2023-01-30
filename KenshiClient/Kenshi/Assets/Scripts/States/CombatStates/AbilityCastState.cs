using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AbilityCastState : FSMState
    {
        private float time = 0.3f;

        public Data data;
        
        public class Data
        {
            public int abilityId;
            public Vector3 hitPoint;
            public Vector3 startPos;
        }
        
        public override FSMStateId Id => FSMStateId.ability_cast;

        public AbilityCastState(Data data)
        {
            this.data = data;
        }
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime > time)
            {
                stateMachine.ChangeState(new IdleState());
            }
            
            Vector3 rot = (data.hitPoint - data.startPos).normalized;
            rot.y = 0;
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(rot); 
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (!stateMachine.Target.GetStat(StatEventPacket.StatId.mana, out ushort v))
            {
                return;
            }

            if (!AbilitiesController.Instance.GetAbilityById(data.abilityId, out var abilityData))
            {
                return;
            }

            var mana = abilityData.Data.mana;
            if (v < mana)
            {
                return;
            }

            if (stateMachine.IsLocal)
            {
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, data), DeliveryMethod.ReliableOrdered);
            }
            else
            {
                if (!GameServer.IsServer)
                {
                    stateMachine.Target.Interpolation.enabled = false;
                }
            }

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, data), DeliveryMethod.ReliableOrdered);
            }
            
            v -= mana;
            CombatController.Instance.SetPlayerStat(new StatEventPacket.Data()
            {
                value = v,
                maxValue = (ushort)100,
                playerId = stateMachine.Target.NetworkId,
                statId = StatEventPacket.StatId.mana
            });
            
            var ability = GameObject.FindObjectOfType<AbilitiesController>().CastAbility(new AbilityInfo()
            {   
                abilityId = data.abilityId,
                user = stateMachine.Target,
                hitPoint = data.hitPoint,
            });

            if (stateMachine.Variables.Grounded)
            {
                time = ability.groundDuration;

                stateMachine.Target.movementStateMachine.ChangeState(new StandState());
                stateMachine.Variables.IsAttacking = true;
            }
            else
            {
                time = ability.airDuration;
            }

            Vector3 rot = (data.hitPoint - data.startPos).normalized;
            rot.y = 0;
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(rot); 
            stateMachine.Target.transform.position = data.startPos; 
            
            // var animator = stateMachine.Variables.Animator;
            // if (animator != null)
            // {
            //     animator.SetTrigger("ability");
            //     animator.SetInteger("ability_id", 1);
            // }

            
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            var animator = stateMachine.Variables.Animator;

            if (animator != null)
            {
                animator.SetTrigger("ability");
                animator.SetInteger("ability_id", 0);
            }

            stateMachine.Variables.IsAttacking = false;
            
            if (!GameServer.IsServer && !stateMachine.Target.IsLocalPlayer)
            {
                stateMachine.Target.Interpolation.enabled = true;
            }
        }
    }
}