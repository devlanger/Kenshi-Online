using UnityEngine;

namespace StarterAssets
{
    public class StandState : FSMState
    {
        private ThirdPersonController tpsController;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
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