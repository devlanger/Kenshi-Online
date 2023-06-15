using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class JumpState : FSMState
    {
        public ThirdPersonController tpsController { get; set; }

        int _animIDJump = Animator.StringToHash("Jump");

        public override FSMStateId Id => FSMStateId.jump;

        public override bool Validate(PlayerStateMachine machine)
        {
            switch (machine.Target.playerStateMachine.CurrentState.Id)
            {
                case FSMStateId.hit:
                case FSMStateId.stunned:
                case FSMStateId.dead:
                    return false;
            }

            return base.Validate(machine);
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            var velocity = tpsController.GetVelocity();
            velocity.y = 0;

            if (velocity != Vector3.zero)
            {
                Vector3 forward = stateMachine.Target.Input.CameraForward;
                Vector3 toOther = velocity;

                if (Vector3.Dot(forward, toOther) > -0.1f)
                {
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(velocity);
                }
                else
                {
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(-velocity);
                }
            }
            else
            {
                stateMachine.Target.transform.rotation =
                    Quaternion.LookRotation(stateMachine.Target.Input.CameraForward);
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

            stateMachine.Target.animator?.SetBool("Grounded", false);
            stateMachine.Target.animator?.SetTrigger("jump");
            stateMachine.Target.animator?.SetInteger("jump_id", stateMachine.Variables.jumpIndex);
        }


        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.animator?.SetBool(_animIDJump, false);
            stateMachine.Target.animator?.SetTrigger("jump");
            stateMachine.Target.animator?.SetInteger("jump_id", 0);
        }
    }
}