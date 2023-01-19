using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using Riptide;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AttackState : FSMState
    {
        public override FSMStateId Id => FSMStateId.attack;

        public Data data;

        private bool damaged = false;
        private float damageTime = 0.25f;
        
        public class Data
        {
            public Vector3 pos;
            public float rot;
        }

        public AttackState()
        {
            
        }
        
        public AttackState(Data data)
        {
            this.data = data;
        }
        
        private bool UpdateAttackInput(PlayerStateMachine machine)
        {
            if (machine.Target.Input.leftClick)
            {
                machine.ChangeState(new AttackState(new Data()
                {
                    pos = machine.Target.transform.position,
                    rot = 0
                }));
                return true;
            }

            return false;
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            bool lastAttack = stateMachine.Variables.attackIndex == 0;
            if (ElapsedTime > (lastAttack ? 0.6f : 0.3f))
            {
                if (!UpdateAttackInput(stateMachine))
                {
                    stateMachine.ChangeState(new IdleState());
                }  
            }
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (GameServer.IsServer)
            {
                CheckDamage(stateMachine);
            }
            
            bool lastAttack = stateMachine.Variables.attackIndex == 0;
            if (ElapsedTime > (lastAttack ? 1 : 0.6f))
            {
                stateMachine.ChangeState(new IdleState());
            }
            else
            {
                stateMachine.Target.transform.position += stateMachine.Target.transform.forward * Time.deltaTime;
            }
        }

        private void CheckDamage(PlayerStateMachine machine)
        {
            if (damaged)
            {
                return;
            }

            //Delayed hit time on the server by ping
            if (ElapsedTime < damageTime - Mathf.Min(machine.Ping, damageTime))
            {
                return;
            }

            var c = Physics.OverlapSphere(machine.Target.transform.position, 4);
            foreach (var pCollider in c)
            {
                var hitTarget = pCollider.GetComponent<Player>();
                if (hitTarget != null && hitTarget.gameObject != machine.Target.gameObject)
                {
                    // float angle = Vector3.Angle(c.transform.position - machine.transform.position, machine.transform.forward);
                    // if (angle > attackAngle / 2)
                    // {
                    //     continue;
                    // }
                    GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(hitTarget.NetworkId, new HitState.Data
                    {
                        attackerId = machine.Target.NetworkId,
                        targetId = hitTarget.NetworkId,
                        hitPos = hitTarget.transform.position,
                        direction = (hitTarget.transform.position - machine.Target.transform.position).normalized * 2
                    }));
                }
            }
            damaged = true;
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, data));
            }

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, new AttackState.Data
                {
                    pos = stateMachine.Target.transform.position,
                    rot = 0,
                }));
            }
            
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            stateMachine.Target.transform.position = data.pos;
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward); 
            stateMachine.Variables.IsAttacking = true;
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
            
            stateMachine.Variables.lastAttackTime = Time.time;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("attack");
                stateMachine.Target.animator.SetInteger("attack_id", 0);
            }
            stateMachine.Variables.IsAttacking = false;
        }
    }
}