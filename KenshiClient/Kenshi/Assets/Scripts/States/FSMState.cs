using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    [System.Serializable]
    public abstract class FSMState
    {
        public abstract FSMStateId Id { get; }
        
        public float ElapsedTime => Time.time - startTime;
        private float startTime;

        private bool entered = false;
        
        public void Enter(PlayerStateMachine stateMachine)
        {
            startTime = Time.time;
            OnEnter(stateMachine);
            entered = true;
        }
        
        public void UpdateInput(PlayerStateMachine stateMachine)
        {
            OnInputUpdate(stateMachine);
        }
        
        public void Update(PlayerStateMachine stateMachine)
        {
            OnUpdate(stateMachine);
        }
        
        public void FixedUpdate(PlayerStateMachine stateMachine)
        {
            OnFixedUpdate(stateMachine);
        }
        
        public void Exit(PlayerStateMachine stateMachine)
        {
            OnExit(stateMachine);
        }
        
        protected abstract void OnUpdate(PlayerStateMachine stateMachine);
        protected abstract void OnInputUpdate(PlayerStateMachine stateMachine);

        protected virtual void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected abstract void OnEnter(PlayerStateMachine stateMachine);
        protected abstract void OnExit(PlayerStateMachine stateMachine);
    }
}