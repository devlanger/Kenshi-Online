using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class IdleState : FSMState
    {
        public override FSMStateId Id => FSMStateId.idle;

        private bool UpdateAttackInput(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.Input.leftClick)
            {
                stateMachine.ChangeState(new AttackState()
                {
                    data = new AttackState.Data { pos = stateMachine.Target.transform.position }
                });
                return true;
            }

            return false;
        }
        
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Variables.attackIndex > 0 && Time.time > stateMachine.Variables.lastAttackTime + 1)
            {
                stateMachine.Variables.attackIndex = 0;
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            UpdateAttackInput(stateMachine);
            if (stateMachine.Target.Input.rightClick)
            {
                stateMachine.ChangeState(new AbilityCastState(new AbilityCastState.Data
                {
                    abilityId = 1,
                    hitPoint = stateMachine.Target.Input.AimDirection,
                    startPos = stateMachine.Target.transform.position,
                }));
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