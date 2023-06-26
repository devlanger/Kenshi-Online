using System.Collections;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using Kenshi.Utils;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AttackState : FSMState
    {
        public override FSMStateId Id => FSMStateId.attack;

        public Data data;

        private bool damaged = false;
        private float damageTime = 0.2f;

        private float _duration = 0.7f;
        private float heavyAttackDuration = 1.1f;
        private float hitDistance = 1.75f;

        public class Data
        {
            public Vector3 pos = Vector3.zero;
            public float rot = 0;
            public bool dashForwardAttack = false;
        }

        public AttackState()
        {
            
        }

        public override bool Validate(PlayerStateMachine stateMachine)
        {
            if(stateMachine.Variables.finishedAirCombo)
            {
                return false;
            }
            
            return base.Validate(stateMachine);
        }

        public AttackState(Data data)
        {
            this.data = data;
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward); 
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                CheckDamage(stateMachine);
            }
            
            var attackDuration = GetAttackDuration(stateMachine);
            if (!(ElapsedTime < attackDuration)) return;
            if (!data.dashForwardAttack)
            {
                stateMachine.Target.tps.SetVelocity(stateMachine.Target.transform.forward * 1f);
            }
            else
            {
                stateMachine.Target.tps.SetVelocity(stateMachine.Target.transform.forward * 5f);
            }
            stateMachine.Target.tps.UpdateGravity();
        }

        public float GetAttackDuration(PlayerStateMachine stateMachine)
        {
            bool lastAttack = stateMachine.Variables.attackIndex == 0;
            float attackDuration = lastAttack ? heavyAttackDuration : _duration;
            return attackDuration;
        }

        private void CheckDamage(PlayerStateMachine machine)
        {
            if (damaged)
            {
                return;
            }

            //Delayed hit time on the server by ping
             if (ElapsedTime < damageTime - Mathf.Min(machine.Target.Ping, damageTime))
             {
                 return;
             }
             
            bool lastAttack = machine.Variables.attackIndex == 0;
            var c = Physics.OverlapSphere(machine.Target.transform.position, hitDistance);
            foreach (var pCollider in c)
            {
                var hitTarget = pCollider.GetComponent<Player>();
                if (hitTarget != null && hitTarget.gameObject != machine.Target.gameObject)
                {
                    switch (hitTarget.playerStateMachine.CurrentState.Id)
                    {
                        case FSMStateId.dead:
                        case FSMStateId.block:
                            continue;
                    }

                    // float angle = Vector3.Angle(c.transform.position - machine.transform.position, machine.transform.forward);
                    // if (angle > attackAngle / 2)
                    // {
                    //     continue;
                    // }
            
                    Vector3 dir = (hitTarget.transform.position - machine.Target.transform.position);
                    dir.y = 0;

                    CombatController.Instance.HitSingleTarget(new DamageData
                    {
                        attacker = machine.Target,
                        hitTarget = hitTarget,
                        damage = 8,
                        direction = lastAttack ? (dir.normalized * 15) : (dir.normalized * 0.5f),
                        hitType = GetHitType(lastAttack)
                    });
                }
            }
            damaged = true;
        }

        private DamageData.HitType GetHitType(bool lastAttack)
        {
            var hitType = lastAttack ? DamageData.HitType.heavy : DamageData.HitType.light;
            if (data.dashForwardAttack)
            {
                hitType = DamageData.HitType.stun;
            }
            
            return hitType;
        }

        public class DamageData
        {
            public Player attacker;
            public Player hitTarget;
            public ushort damage;
            public Vector3 direction;
            public HitType hitType;
            
            public enum HitType
            {
                very_light = 0,
                light = 1,
                heavy = 2,
                stun = 3,
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, data), DeliveryMethod.ReliableOrdered);
            }

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, new AttackState.Data
                {
                    pos = stateMachine.Target.transform.position,
                    rot = 0,
                    dashForwardAttack = data.dashForwardAttack
                }), DeliveryMethod.ReliableOrdered);
            }

            if (!data.dashForwardAttack)
            {
                stateMachine.Target.movementStateMachine.ChangeState(new FreezeMoveState());
            }

            if (SfxController.Instance != null)
            {
                var clip = SfxController.Instance.manager.GetRandomMeleeWooshSfx();
                SfxController.Instance.PlaySound(clip.sfxId, 0.5f);
            }
            
            stateMachine.Target.transform.position = data.pos;
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward); 
            stateMachine.Variables.IsAttacking = true;
            stateMachine.Variables.lastAttackTime = Time.time;
            stateMachine.Variables.attackIndex++;
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("attack");
                stateMachine.Target.animator.SetInteger("attack_id", stateMachine.Variables.attackIndex);
            }

            if (stateMachine.Variables.attackIndex == 4)
            {
                stateMachine.Variables.attackIndex = 0;
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("attack");
                stateMachine.Target.animator.SetInteger("attack_id", 0);
            }

            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            stateMachine.Target.tps.SetVelocity(Vector3.zero, true);
            stateMachine.Variables.IsAttacking = false;

            if(!stateMachine.Target.tps.Grounded && stateMachine.Variables.attackIndex == 0)
            {
                stateMachine.Variables.finishedAirCombo = true;
            }
        }
    }
}