using UnityEngine;

namespace StarterAssets
{
    public abstract class FSMState
    {
        public float ElapsedTime => Time.time - startTime;
        private float startTime;
        
        public void Enter(PlayerStateMachine stateMachine)
        {
            startTime = Time.time;
            OnEnter(stateMachine);
        }
        
        public void Update(PlayerStateMachine stateMachine)
        {
            OnUpdate(stateMachine);
        }
        
        public void Exit(PlayerStateMachine stateMachine)
        {
            OnExit(stateMachine);
        }
        
        protected abstract void OnUpdate(PlayerStateMachine stateMachine);
        protected abstract void OnEnter(PlayerStateMachine stateMachine);
        protected abstract void OnExit(PlayerStateMachine stateMachine);
    }
}