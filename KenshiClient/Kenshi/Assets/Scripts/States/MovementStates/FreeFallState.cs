using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class FreeFallState : FSMState
    {
        public override FSMStateId Id => FSMStateId.freefall;
        private ThirdPersonController tps;

        int _animIDFreeFall = Animator.StringToHash("FreeFall");
        int _animIDGrounded = Animator.StringToHash("Grounded");
        
        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
            {
                stateMachine.ChangeState(new JumpState());
            }
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (tps.GroundedCheck())
            {
                stateMachine.ChangeState(new LandState());
            }
            else
            {
                if(stateMachine.IsLocal)
                {
                    if (!tps.GroundedCheck())
                    {
                        tps.UpdateGravity();
                        var velocity = tps.GetVelocity();
                        tps.UpdateMovement(velocity);
                    }
                }
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tps = stateMachine.Target.tps;
            stateMachine.Target.animator?.SetBool(_animIDFreeFall, true);
            stateMachine.Target.animator?.SetBool(_animIDGrounded, false);
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.animator?.SetBool(_animIDFreeFall, false);
        }
    }
}