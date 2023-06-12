using Kenshi.Shared.Enums;
using UnityEngine;

namespace StarterAssets
{
    public class StandState : FSMState
    {
        public override FSMStateId Id => FSMStateId.stand;

        private ThirdPersonController tpsController;

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.transform.rotation = Quaternion.LookRotation(stateMachine.Target.Input.CameraForward);
            
            // stateMachine.Target.transform.rotation = Quaternion.Slerp(stateMachine.Target.transform.rotation, 
            //     Quaternion.LookRotation(stateMachine.Target.Input.CameraForward), Time.deltaTime * 5); 
            
            switch (stateMachine.Target.playerStateMachine.CurrentState.Id)
            {
                case FSMStateId.idle:
                    if (stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
                    {
                        stateMachine.ChangeState(new DashState(new DashState.Data
                        {
                            dashIndex = stateMachine.Target.Input.dashIndex
                        }));
                    }
                    else if (stateMachine.Target.Input.move != Vector2.zero)
                    {
                        stateMachine.ChangeState(new MoveState());
                    }
                    else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
                    {
                        stateMachine.ChangeState(new JumpState());
                    }
                    
                    break;
            }
        }

        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
            if (!tpsController.Grounded)
            {
                stateMachine.ChangeState(new FreeFallState());
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                if (tpsController._verticalVelocity < 0)
                {
                    tpsController._verticalVelocity = 0f;
                }
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            tpsController = stateMachine.Target.tps;
            tpsController.StopMoving();
            stateMachine.Variables.Jumping = true;
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
        }
    }
}