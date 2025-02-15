using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets.CombatStates
{
    public class DeadState : FSMState
    {
        public override FSMStateId Id => FSMStateId.dead;

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(false);
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, Id), DeliveryMethod.ReliableOrdered);
            }
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", true);
            }
            
            stateMachine.Target.movementStateMachine.ChangeState(new FreezeMoveState());
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(true);
            }
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", false);
            }
            
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
        }
    }
}