using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class MoveState : FSMState
    {
        public override FSMStateId Id => FSMStateId.move;

        private ThirdPersonController tpsController;

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
            var velocity = GetDirection(stateMachine);
            velocity.y = 0;

            Vector3 forward = stateMachine.Target.Input.CameraForward;
            Vector3 toOther = velocity;
            
            if (velocity != Vector3.zero && Vector3.Dot(forward, toOther) > -0.1f)
            {
                stateMachine.Target.transform.rotation = Quaternion.LookRotation(velocity);
            }
            else
            {
                stateMachine.Target.transform.rotation = Quaternion.LookRotation(-velocity);
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController == null || !stateMachine.IsBot && !stateMachine.IsLocal)
            {
                return;
            }
            
            if (tpsController._verticalVelocity < 0)
            {
                tpsController._verticalVelocity = 0f;
            }
            
            tpsController.UpdateGravity();

            var velocity = GetDirection(stateMachine);
            tpsController.UpdateMovement(velocity);
        }

        private Vector3 GetDirection(PlayerStateMachine stateMachine)
        {
            var velocity = tpsController.GetVelocity();
            if (Physics.Raycast(stateMachine.Target.transform.position, -stateMachine.Target.transform.up, out RaycastHit hit,
                    0.1f, stateMachine.Variables.GroundLayers))
            {
                Vector3 forward = GetForwardTangent(velocity, hit.normal);
                velocity = forward.normalized * GetSpeed(stateMachine);
            }

            return velocity;
        }

        private float GetSpeed(PlayerStateMachine stateMachine)
        {
            var speed = tpsController.MoveSpeed;
            if (stateMachine.Target.Input.IsSprinting())
            {
                speed = tpsController.SprintSpeed;
            }
            
            return speed;
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