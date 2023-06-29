using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class StandState : FSMState
    {
        public override FSMStateId Id => FSMStateId.stand;

        private ThirdPersonController tpsController;

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            if(stateMachine.Target.Input.CameraForward != Vector3.zero)
                stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward);
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                if (tpsController._verticalVelocity < 0)
                {
                    tpsController._verticalVelocity = 0f;
                }
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tpsController = stateMachine.Target.tps;
            tpsController.StopMoving();
            stateMachine.Variables.Jumping = true;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}