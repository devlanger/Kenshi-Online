using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets
{
    public abstract class GenericFSMState<T> : FSMState
    {
        public abstract T StateId { get; }
    }
    
    [System.Serializable]
    public abstract class FSMState
    {
        public abstract FSMStateId Id { get; }

        public FSMState lastState;
        
        public float ElapsedTime => Time.time - startTime;
        private float startTime;

        private bool entered = false;

        public virtual bool Validate(PlayerStateMachine machine) => true;
        
        public void Enter(PlayerStateMachine stateMachine, FSMState lastState = null)
        {
            this.lastState = lastState; 
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
        
        public void LateUpdate(PlayerStateMachine stateMachine)
        {
            OnLateUpdate(stateMachine);
        }
        
        public void Exit(PlayerStateMachine stateMachine)
        {
            OnExit(stateMachine);
        }

        protected void SyncStateOverNetwork(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, Id), DeliveryMethod.ReliableOrdered);
            }

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, Id), DeliveryMethod.ReliableOrdered);
            }
        }

        protected virtual void OnUpdate(PlayerStateMachine stateMachine){}
        protected virtual void OnInputUpdate(PlayerStateMachine stateMachine){}

        protected virtual void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected virtual void OnLateUpdate(PlayerStateMachine stateMachine)
        {
        }
        protected abstract void OnEnter(PlayerStateMachine stateMachine);
        protected abstract void OnExit(PlayerStateMachine stateMachine);
    }
}