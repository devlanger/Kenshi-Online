using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets.CombatStates
{
    public class HitState : FSMState
    {
        public override FSMStateId Id => FSMStateId.hit;

        private Data data;
        
        int _animIDJump = Animator.StringToHash("Jump");
        
        public class Data
        {
            public int attackerId;
            public int targetId;
            public Vector3 hitPos;
            public Vector3 direction;
            public float duration;
        }
        
        public HitState()
        {
            
        }

        public HitState(Data hitData)
        {
            this.data = hitData;
        }
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime > data.duration)
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 1);
            }

            if (GameServer.IsServer)
            {
                var agent = stateMachine.Target.GetComponent<NavMeshAgent>();
                if (agent)
                {
                    agent.enabled = false;
                }
            }

            if (!GameServer.IsServer && !stateMachine.Target.Interpolation.enabled)
            {
                //stateMachine.Target.Interpolation.enabled = false;
            }

            if (GameServer.IsServer || stateMachine.IsLocal)
            {
                stateMachine.Target.transform.position = data.hitPos;
                Vector3 rot = -data.direction;
                rot.y = 0;
                stateMachine.Target.transform.rotation = Quaternion.LookRotation(rot);
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer || stateMachine.IsLocal)
            {
                if (ElapsedTime < 0.3f)
                {
                    stateMachine.Target.tps.SetVelocity(data.direction);
                }
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.tps.StopMoving();
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 0);
            }

            if (GameServer.IsServer)
            {
                var agent = stateMachine.Target.GetComponent<NavMeshAgent>();
                if (agent)
                {
                    agent.enabled = true;
                }
            }
            
            if (!GameServer.IsServer && !stateMachine.IsLocal)
            {
                //stateMachine.Target.Interpolation.enabled = true;
            }
        }
    }
}