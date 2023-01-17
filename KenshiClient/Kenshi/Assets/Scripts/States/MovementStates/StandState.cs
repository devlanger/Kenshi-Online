using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class StandState : FSMState
    {
        public override FSMStateId Id => FSMStateId.stand;

        private ThirdPersonController tpsController;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController == null || !stateMachine.IsLocal)
            {
                return;
            }
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