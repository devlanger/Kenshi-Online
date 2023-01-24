using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class DeadState : FSMState
    {
        public override FSMStateId Id => FSMStateId.dead;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", true);
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", false);
            }
        }
    }
}