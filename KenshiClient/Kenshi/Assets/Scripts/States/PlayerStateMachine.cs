using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    [System.Serializable]
    public class PlayerStateMachine
    {
        public Player Target;

        public bool IsLocal => Target.IsLocalPlayer;
        public bool IsBot => Target.IsBot;
        
        public FSMState CurrentState;
        public StateMachineVariables Variables;
        public float Ping => Target != null ? Target.Ping : 0f;

        public Queue<DelayedState> queuedStates = new Queue<DelayedState>();

        public class DelayedState
        {
            public float dequeueTime;
            public FSMState state;
        }

        public event Action<FSMState> OnStateEnter;

        public void ChangeState(FSMState newState, float delayTime = 0)
        {
            try
            {
                ExecuteStateChange(newState);
               //queuedStates.Enqueue(new DelayedState{state = newState, dequeueTime = Time.time + delayTime});
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                //throw;
            }
        }

        private void ExecuteStateChange(FSMState newState)
        {
            //Debug.Log(newState);
            if (!newState.Validate(this))
            {
                return;
            }

            var oldState = CurrentState;

            if (CurrentState != null)
                CurrentState.Exit(this);

            CurrentState = newState;

            if (newState != null)
            {
                newState.Enter(this, oldState);
                OnStateEnter?.Invoke(newState);
            }
        }

        public void UpdateQueue()
        {
            if (queuedStates.Count > 0)
            {
                var peeked = queuedStates.Peek();
                if (Time.time > peeked.dequeueTime)
                {
                    queuedStates.Dequeue();
                    ExecuteStateChange(peeked.state);
                }
            }
        }
    }
}