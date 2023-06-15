using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class FreezeMoveState : FSMState
    {
        public override FSMStateId Id => FSMStateId.freeze;

        private ThirdPersonController tpsController;

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