using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;

namespace StarterAssets.CombatStates
{
    public class StunState : FSMState
    {
        public override FSMStateId Id => FSMStateId.stunned;

        private float duration = 1;
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime > duration)
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(false);
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, Id), DeliveryMethod.ReliableOrdered);
            }
         
            //TODO: Spawn stun particles
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 10);
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
        }
    }
}