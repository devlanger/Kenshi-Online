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
            if (!tpsController.Grounded)
            {
                stateMachine.ChangeState(new FreeFallState());
            }
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
            else if (stateMachine.Target.Input.move == Vector2.zero)
            {
                stateMachine.ChangeState(new StandState());
            }
            else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
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
            
            if (tpsController._verticalVelocity < 0)
            {
                tpsController._verticalVelocity = 0f;
            }
            
            tpsController.UpdateGravity();

            var velocity = tpsController.GetVelocity();
            if(Physics.Raycast(stateMachine.Target.transform.position, -stateMachine.Target.transform.up, out RaycastHit hit, 0.1f, stateMachine.Variables.GroundLayers))
            {
                Vector3 forward = GetForwardTangent(velocity,hit.normal);
                velocity = forward.normalized * (stateMachine.Target.Input.sprint ? tpsController.SprintSpeed : tpsController.MoveSpeed);
            }
            
            tpsController.UpdateMovement(velocity);
        }

        public Vector3 GetForwardTangent(Vector3 moveDir, Vector3 up)
        {
            Vector3 rght = Vector3.Cross(up,moveDir);
            Vector3 forw = Vector3.Cross(rght, up);
            return forw;
        }
        
        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tpsController = stateMachine.Target.tps;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            
        }
    }
}