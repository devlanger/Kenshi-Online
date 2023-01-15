using UnityEngine;

namespace StarterAssets
{
    public class DashState : FSMState
    {
        public DashState()
        {
            
        }
        
        protected override void OnUpdate(PlayerStateMachine machine)
        {
            if (ElapsedTime > 0.4f)
            {
                machine.ChangeState(new StandState());
            }
            machine.Target.transform.position -= Camera.main.transform.forward * 10 * Time.deltaTime;
        }

        protected override void OnEnter(PlayerStateMachine machine)
        {
        }

        protected override void OnExit(PlayerStateMachine machine)
        {
        }
    }
}