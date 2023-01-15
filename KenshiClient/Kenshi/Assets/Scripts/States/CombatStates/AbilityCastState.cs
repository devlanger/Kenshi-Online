using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class AbilityCastState : FSMState
    {
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (ElapsedTime > 0.3f)
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            var animator = stateMachine.Variables.Animator;
            Physics.Raycast(Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition), out RaycastHit hit, 100, stateMachine.Variables._aimLayerMask);
            animator.SetTrigger("ability");
            animator.SetInteger("ability_id", 1);
            GameObject.FindObjectOfType<AbilitiesController>().CastAbility(new AbilityInfo()
            {   
                user = stateMachine.Target,
                abilityId = 1,
                aimPoint = hit.collider == null ? Camera.main.transform.forward : hit.point
            });
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            var animator = stateMachine.Variables.Animator;

            animator.SetTrigger("ability");
            animator.SetInteger("ability_id", 0);
        }
    }
}