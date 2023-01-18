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
            switch (stateMachine.Target.playerStateMachine.CurrentState.Id)
            {
                case FSMStateId.idle:
                    if (stateMachine.Target.Input.jump)
                    {
                        stateMachine.ChangeState(new JumpState());
                    }
                    
                    if (stateMachine.Target.Input.move != Vector2.zero)
                    {
                        stateMachine.ChangeState(new MoveState());
                    }
                    break;
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
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