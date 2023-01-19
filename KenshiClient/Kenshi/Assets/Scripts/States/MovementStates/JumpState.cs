using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class JumpState : FSMState
    {
        public ThirdPersonController tpsController { get; set; }

        int _animIDJump = Animator.StringToHash("Jump");
        
        public override FSMStateId Id { get; }
        
        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
            {
                stateMachine.ChangeState(new JumpState());
            }
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController._verticalVelocity < 0)
            {
                stateMachine.ChangeState(new FreeFallState());
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController == null || !stateMachine.IsLocal)
            {
                return;
            }

            tpsController.UpdateGravity();
            var velocity = tpsController.GetVelocity();
            tpsController.UpdateMovement(velocity);
    }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.animator?.SetBool(_animIDJump, true);
            if (stateMachine.IsLocal)
            {
                tpsController = stateMachine.Target.tps;
                stateMachine.Target.Input.jump = false;
                stateMachine.Variables.jumpIndex++;
                tpsController.SetVerticalVelocity();
            }
        }


        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.animator?.SetBool(_animIDJump, false);
        }
    }
}