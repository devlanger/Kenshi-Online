using Kenshi.Shared.Enums;
using UnityEngine;

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
            if (ElapsedTime > 0.4f)
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

            if (!GameServer.IsServer && !stateMachine.IsLocal)
            {
                stateMachine.Target.Interpolation.enabled = false;
            }

            stateMachine.Target.transform.position = data.hitPos;
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime < 0.2f)
            {
                stateMachine.Target.transform.position += data.direction * Time.deltaTime;
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", 0);
            }

            if (!GameServer.IsServer && !stateMachine.IsLocal)
            {
                stateMachine.Target.Interpolation.enabled = true;
            }
        }
    }
}