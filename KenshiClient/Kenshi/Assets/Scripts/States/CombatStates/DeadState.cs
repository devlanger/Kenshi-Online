using Kenshi.Shared.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets.CombatStates
{
    public class DeadState : FSMState
    {
        public override FSMStateId Id => FSMStateId.dead;

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(false);
            }
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", true);
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                stateMachine.Target.ActivateNavAgent(true);
            }
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("dead", false);
            }
        }
    }
}