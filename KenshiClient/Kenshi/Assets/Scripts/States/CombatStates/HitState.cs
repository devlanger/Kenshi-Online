using Kenshi.Shared.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets.CombatStates
{
    public class HitState : FSMState
    {
        public override FSMStateId Id => FSMStateId.hit;

        public Data data;
        
        int _animIDJump = Animator.StringToHash("Jump");
        
        public class Data
        {
            public int attackerId;
            public int targetId;
            public Vector3 hitPos;
            public Vector3 direction;
            public float duration;
            public AttackState.DamageData.HitType hitType;
        }
        
        public HitState()
        {
            
        }

        public HitState(Data hitData)
        {
            this.data = hitData;
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.movementStateMachine.ChangeState(new FreezeMoveState());
            
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("hit");
                stateMachine.Target.animator.SetInteger("hit_id", data.hitType == AttackState.DamageData.HitType.heavy ? 2 : 1);
            }

            if (GameServer.IsServer)
            {
                var agent = stateMachine.Target.GetComponent<NavMeshAgent>();
                if (agent)
                {
                    agent.enabled = false;
                }
            }
            else
            {
                Debug.Log(data.hitType);
                switch (data.hitType)
                {
                    case AttackState.DamageData.HitType.light:
                    case AttackState.DamageData.HitType.very_light:
                    case AttackState.DamageData.HitType.stun:
                        VfxController.Instance.SpawnFx(VfxController.VfxId.hit_light, stateMachine.Target.transform.position + Vector3.up, Quaternion.identity);
                        break;
                    case AttackState.DamageData.HitType.heavy:
                        VfxController.Instance.SpawnFx(VfxController.VfxId.hit_heavy, stateMachine.Target.transform.position + Vector3.up, stateMachine.Target.transform.rotation);
                        break;
                }

                if (SfxController.Instance != null)
                {
                    var clip = SfxController.Instance.manager.GetRandomMeleeHitSfx();
                    SfxController.Instance.PlaySound(clip.sfxId, 0.5f);
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
                stateMachine.Target.tps.UpdateGravity();

                if (ElapsedTime < 0.3f)
                {
                    if (data.hitType != AttackState.DamageData.HitType.stun)
                    {
                        stateMachine.Target.tps.SetVelocity(data.direction);
                    }
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
            
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
        }
    }
}