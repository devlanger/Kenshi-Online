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
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (tpsController == null || !stateMachine.IsLocal)
            {
                return;
            }

            if (stateMachine.Variables.IsAttacking)
            {
                stateMachine.Target.Input.move = Vector2.zero;
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