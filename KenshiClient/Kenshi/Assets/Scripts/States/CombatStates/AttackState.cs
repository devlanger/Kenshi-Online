using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AttackState : FSMState
    {
        public override FSMStateId Id => FSMStateId.attack;

        private bool UpdateAttackInput(PlayerStateMachine machine)
        {
            if (!machine.IsLocal)
            {
                return false;
            }
            
            if (machine.Target.Input.leftClick)
            {
                machine.ChangeState(new AttackState());
                return true;
            }

            return false;
        }


        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.transform.position += stateMachine.Target.transform.forward * Time.deltaTime;

            bool lastAttack = stateMachine.Variables.attackIndex == 0;
            if (ElapsedTime > (lastAttack ? 0.6f : 0.3f))
            {
                if (!UpdateAttackInput(stateMachine))
                {
                    if (ElapsedTime > (lastAttack ? 1 : 0.6f))
                    {
                        stateMachine.ChangeState(new IdleState());
                    }  
                }
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward); 
            stateMachine.Variables.IsAttacking = true;
            stateMachine.Variables.attackIndex++;
            stateMachine.Target.GetComponent<Animator>()?.SetTrigger("attack");
            stateMachine.Target.GetComponent<Animator>()?.SetInteger("attack_id", stateMachine.Variables.attackIndex);
            if (stateMachine.Variables.attackIndex == 4)
            {
                stateMachine.Variables.attackIndex = 0;
            }
            
            stateMachine.Variables.lastAttackTime = Time.time;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.GetComponent<Animator>().SetTrigger("attack");
            stateMachine.Target.GetComponent<Animator>().SetInteger("attack_id", 0);
            stateMachine.Variables.IsAttacking = false;
        }
    }
}