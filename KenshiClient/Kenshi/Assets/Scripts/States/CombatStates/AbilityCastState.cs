using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AbilityCastState : FSMState
    {
        private float time = 0.3f;
        
        public override FSMStateId Id => FSMStateId.ability_cast;

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime > time)
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.Variables.Grounded)
            {
                stateMachine.Target.movementStateMachine.ChangeState(new StandState());
                stateMachine.Variables.IsAttacking = true;
            }
            else
            {
                time = 0.175f;
            }

            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward); 
            
            var animator = stateMachine.Variables.Animator;
            animator.SetTrigger("ability");
            animator.SetInteger("ability_id", 1);
            GameObject.FindObjectOfType<AbilitiesController>().CastAbility(new AbilityInfo()
            {   
                user = stateMachine.Target,
                abilityId = 1,
                aimPoint = stateMachine.Target.Input.AimDirection
            });
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            var animator = stateMachine.Variables.Animator;

            animator.SetTrigger("ability");
            animator.SetInteger("ability_id", 0);
            stateMachine.Variables.IsAttacking = false;
        }
    }
}