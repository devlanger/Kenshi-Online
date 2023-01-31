using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class BlockState : FSMState
    {
        public override FSMStateId Id => FSMStateId.block;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            SyncStateOverNetwork(stateMachine);
            
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("block", true);
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("block", false);
            }
        }
    }
}