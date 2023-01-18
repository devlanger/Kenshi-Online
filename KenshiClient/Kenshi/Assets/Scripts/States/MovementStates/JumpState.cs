using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class JumpState : FSMState
    {
        public ThirdPersonController tpsController { get; set; }

        public override FSMStateId Id { get; }
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Variables.Jumping)
            {
                if (stateMachine.Target.Input.move != Vector2.zero)
                {
                    stateMachine.ChangeState(new MoveState());
                }
                else
                {
                    stateMachine.ChangeState(new StandState());
                }
            }
            else
            {
                if (tpsController == null || !stateMachine.IsLocal)
                {
                    return;
                }

                tpsController.UpdateGravity();
                tpsController.UpdateMovement();
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tpsController = GameObject.FindObjectOfType<ThirdPersonController>();
            tpsController.StopMoving();
        }


        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}