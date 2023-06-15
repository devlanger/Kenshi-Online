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
            var velocity = tps.GetVelocity();
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
            if (!tps.GroundedCheck())
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
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator?.SetBool(_animIDFreeFall, true);
                stateMachine.Target.animator?.SetBool(_animIDGrounded, false);
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator?.SetBool(_animIDFreeFall, false);
                stateMachine.Target.animator?.SetBool(_animIDGrounded, false);
            }
        }
    }
}