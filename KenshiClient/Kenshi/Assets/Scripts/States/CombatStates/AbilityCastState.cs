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
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
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
            
            if (stateMachine.Variables.Grounded)
            {
                stateMachine.Target.movementStateMachine.ChangeState(new StandState());
                stateMachine.Variables.IsAttacking = true;
            }
            else
            {
                time = 0.175f;
            }

            stateMachine.Target.transform.rotation = Quaternion.LookRotation((data.hitPoint - data.startPos).normalized); 
            stateMachine.Target.transform.position = data.startPos; 
            
            var animator = stateMachine.Variables.Animator;
            if (animator != null)
            {
                animator.SetTrigger("ability");
                animator.SetInteger("ability_id", 1);
            }

            GameObject.FindObjectOfType<AbilitiesController>().CastAbility(new AbilityInfo()
            {   
                abilityId = 1,
                user = stateMachine.Target,
                aimPoint = data.hitPoint,
            });
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