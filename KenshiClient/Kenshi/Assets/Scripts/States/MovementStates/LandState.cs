using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class LandState : FSMState
    {
        public override FSMStateId Id => FSMStateId.land;
        private int _animIDGrounded = Animator.StringToHash("Grounded");

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.animator.SetBool(_animIDGrounded, true);
            stateMachine.ChangeState(new StandState());
            stateMachine.Variables.jumpIndex = 0;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}