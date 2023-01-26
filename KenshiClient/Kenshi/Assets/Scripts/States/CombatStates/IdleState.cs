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
            AbilitiesController.Instance.UpdateInputs();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                stateMachine.ChangeState(new BlockState());
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                stateMachine.ChangeState(new ManaRegenState());
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