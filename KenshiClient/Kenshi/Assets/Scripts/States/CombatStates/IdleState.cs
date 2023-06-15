using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class IdleState : FSMState
    {
        public override FSMStateId Id => FSMStateId.idle;
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Variables.attackIndex > 0 && Time.time > stateMachine.Variables.lastAttackTime + 1 && stateMachine.Target.tps.Grounded)
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