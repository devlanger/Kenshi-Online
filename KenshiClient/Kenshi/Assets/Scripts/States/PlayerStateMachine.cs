using UnityEngine;

namespace StarterAssets
{
    [System.Serializable]
    public class PlayerStateMachine
    {
        public Player Target;
        
        public FSMState CurrentState;
        public StateMachineVariables Variables;
        
        public void ChangeState(FSMState newState)
        {
            CurrentState?.Exit(this);
            newState?.Enter(this);
        }
    }
}