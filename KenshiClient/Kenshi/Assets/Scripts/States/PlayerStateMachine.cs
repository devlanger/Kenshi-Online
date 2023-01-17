using System;
using UnityEngine;

namespace StarterAssets
{
    [System.Serializable]
    public class PlayerStateMachine
    {
        public Player Target;

        public bool IsLocal => Target.IsLocalPlayer;
        
        public FSMState CurrentState;
        public StateMachineVariables Variables;
        
        public void ChangeState(FSMState newState)
        {
            try
            {
                Debug.Log(newState);
                
                if(CurrentState != null)
                    CurrentState.Exit(this);
                
                CurrentState = newState;
                
                if(newState != null)
                    newState.Enter(this);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                //throw;
            }
        }
    }
}