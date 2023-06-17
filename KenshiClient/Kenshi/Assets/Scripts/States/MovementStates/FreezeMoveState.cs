using Kenshi.Shared.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets
{
    public class FreezeMoveState : FSMState
    {
        public override FSMStateId Id => FSMStateId.freeze;

        private ThirdPersonController tpsController;

        private bool _initialAgentState;

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.TryGetComponent(out NavMeshAgent agent))
            {
                _initialAgentState = agent.enabled;
                agent.enabled = false;
            }
            
            stateMachine.Target.tps.SetVelocity(Vector3.zero);
            stateMachine.Target.tps?.StopMoving();
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.TryGetComponent(out NavMeshAgent agent))
            {
                agent.enabled = _initialAgentState;
            }
        }
    }
}