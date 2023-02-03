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
            if (stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
            {
                stateMachine.ChangeState(new DashState(new DashState.Data
                {
                    dashIndex = stateMachine.Target.Input.dashIndex
                }));
            }
            else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
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