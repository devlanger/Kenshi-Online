using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class IdleState : FSMState
    {
        public override FSMStateId Id => FSMStateId.idle;

        private bool UpdateAttackInput(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.Input.leftClick && stateMachine.IsLocal)
            {
                stateMachine.ChangeState(new AttackState());
                return true;
            }

            return false;
        }
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            UpdateAttackInput(stateMachine);
            if (stateMachine.Target.Input.rightClick && stateMachine.IsLocal)
            {
                stateMachine.ChangeState(new AbilityCastState());
            }
        
            if (stateMachine.Variables.attackIndex > 0 && Time.time > stateMachine.Variables.lastAttackTime + 1)
            {
                stateMachine.Variables.attackIndex = 0;
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}