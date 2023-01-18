using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class MoveState : FSMState
    {
        public override FSMStateId Id => FSMStateId.move;

        private ThirdPersonController tpsController;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.Input.move == Vector2.zero)
            {
                stateMachine.ChangeState(new StandState());
            }

            if (stateMachine.Target.Input.jump)
            {
                stateMachine.ChangeState(new JumpState());
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController == null || !stateMachine.IsLocal)
            {
                return;
            }

            tpsController.UpdateGravity();
            tpsController.UpdateMovement();
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tpsController = GameObject.FindObjectOfType<ThirdPersonController>();
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}