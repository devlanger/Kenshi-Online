using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class FreezeMoveState : FSMState
    {
        public override FSMStateId Id => FSMStateId.freeze;

        private ThirdPersonController tpsController;

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.tps.SetVelocity(Vector3.zero);
            stateMachine.Target.tps?.StopMoving();
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}