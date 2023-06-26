using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class StunState : FSMState
    {
        public override FSMStateId Id => FSMStateId.stunned;

        public float Duration { get; private set; } = 1;

        private GameObject fx;
        
        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(false);
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, Id), DeliveryMethod.ReliableOrdered);
            }
         
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 10);
            }

            if (!GameServer.IsServer)
            {
                fx = VfxController.Instance.SpawnFx(VfxController.VfxId.stun, stateMachine.Target.transform.position, Quaternion.identity, 
                    stateMachine.Target.transform); 
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if(GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(true);
            }
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 0);
            }

            if (!GameServer.IsServer)
            {
                GameObject.Destroy(fx.gameObject);
            }
        }
    }
}