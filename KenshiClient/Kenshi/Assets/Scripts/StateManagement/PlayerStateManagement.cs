using Kenshi.Shared.Enums;
using StarterAssets.CombatStates;
using UnityEngine;

namespace StarterAssets.StateManagement
{
    [System.Serializable]
    public class PlayerStateManagement
    {
        public FSMStateId StateId;
        
        public void UpdateInputStateManagement(PlayerStateMachine stateMachine)
        {
            var currentState = stateMachine.CurrentState;
            
            switch (currentState)
            {
                case BlockState blockState:
                    if (stateMachine.IsLocal)
                    {
                        if (UnityEngine.Input.GetKeyUp(KeyCode.Q))
                        {
                            stateMachine.ChangeState(new IdleState());
                        }
                    }
                    break;
                case FreeFallState freeFallState:
                    if (stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
                    {
                        stateMachine.ChangeState(new DashState(new DashState.Data
                        {
                            dashIndex = stateMachine.Target.Input.dashIndex
                        }));
                    }
                    else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
                    {
                        stateMachine.ChangeState(new JumpState());
                    }
                    break;
                case AttackState attackState:
                    if (stateMachine.Target.movementStateMachine.CurrentState.Id != FSMStateId.dash && stateMachine.Target.Input.leftClick)
                    {
                        if (attackState.ElapsedTime > attackState.GetAttackDuration(stateMachine) - 0.3f)
                        {
                            stateMachine.ChangeState(new AttackState(new AttackState.Data()
                            {
                                pos = stateMachine.Target.transform.position,
                                rot = 0
                            }));
                        }
                    }
                    break;
                case IdleState idleState:
                    if (stateMachine.Target.Input.leftClick)
                    {
                        if (stateMachine.Target.movementStateMachine.CurrentState is DashState dashState)
                        {
                            if (dashState.data.dashIndex == DashState.Data.DashIndex.forward)
                            {
                                stateMachine.ChangeState(new AttackState()
                                {
                                    data = new AttackState.Data { pos = stateMachine.Target.transform.position, 
                                        dashForwardAttack = dashState.data.dashIndex == DashState.Data.DashIndex.forward }
                                });   
                            }   
                        }
                        else
                        {
                            stateMachine.ChangeState(new AttackState()
                            {
                                data = new AttackState.Data { pos = stateMachine.Target.transform.position }
                            });
                        }
                    }

                    if(stateMachine.IsLocal && stateMachine.Target.movementStateMachine.CurrentState.Id != FSMStateId.dash)
                    {
                        AbilitiesController.Instance.UpdateInputs();
                        if (UnityEngine.Input.GetKey(KeyCode.Q))
                        {
                            stateMachine.ChangeState(new BlockState());
                        }
                        else if (UnityEngine.Input.GetKey(KeyCode.R))
                        {
                            stateMachine.ChangeState(new ManaRegenState());
                        }
                    }
                    break;
                case JumpState jumpState:
                    if (stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
                    {
                        stateMachine.ChangeState(new DashState(new DashState.Data
                        {
                            dashIndex = stateMachine.Target.Input.dashIndex
                        }));
                    }
                    else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
                    {
                        stateMachine.ChangeState(new JumpState());
                    }
                    else
                    {
                        stateMachine.Target.Input.jump = false;
                    }
                    break;
                case DashState dashState:
                    if (dashState.ElapsedTime > dashState.Duration - 0.15f && stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
                    {
                        stateMachine.ChangeState(new DashState(new DashState.Data
                        {
                            dashIndex = stateMachine.Target.Input.dashIndex
                        }));
                    }
                    break;
                case ManaRegenState manaRegenState:
                    if (stateMachine.IsLocal)
                    {
                        if (UnityEngine.Input.GetKeyUp(KeyCode.R))
                        {
                            stateMachine.ChangeState(new IdleState());
                        }
                    }

                    break;
                case MoveState moveState:
                    if (stateMachine.Target.Input.dashIndex != DashState.Data.DashIndex.none)
                    {
                        stateMachine.ChangeState(new DashState(new DashState.Data
                        {
                            dashIndex = stateMachine.Target.Input.dashIndex
                        }));
                    }
                    else if (stateMachine.Target.Input.move == Vector2.zero)
                    {
                        stateMachine.ChangeState(new StandState());
                    }
                    else if (stateMachine.Target.Input.jump && stateMachine.Variables.jumpIndex < 2)
                    {
                        stateMachine.ChangeState(new JumpState());
                    }
                    break;
                case StandState standState:
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

                    break;
            }
        }

        public void UpdateStateManagement(PlayerStateMachine stateMachine)
        {
            var currentState = stateMachine.CurrentState;
            StateId = currentState.Id;

            switch (currentState)
            {
                case HitState hitState:
                    if (hitState.ElapsedTime > hitState.data.duration)
                    {
                        stateMachine.ChangeState(new IdleState());
                    }

                    break;
                case AbilityCastState abilityCastState:
                    if (abilityCastState.ElapsedTime > abilityCastState.time)
                    {
                        stateMachine.ChangeState(new IdleState());
                    }

                    break;
                case AttackState attackState:
                    if (attackState.ElapsedTime > attackState.GetAttackDuration(stateMachine))
                    {
                        stateMachine.ChangeState(new IdleState());
                    }
                    break;
                case DashState dashState:
                    if (dashState.ElapsedTime > dashState.Duration)
                    {
                        if (stateMachine.Variables.Grounded && dashState.StartedInAir)
                        {
                            stateMachine.ChangeState(new LandState());
                        }
                        else
                        {
                            stateMachine.ChangeState(new StandState());
                        }
                    }

                    break;
                case FreeFallState freeFallState:
                    if (stateMachine.Target.tps.GroundedCheck())
                    {
                        stateMachine.ChangeState(new LandState());
                    }

                    break;
                case JumpState jumpState:
                    if (stateMachine.Target.tps._verticalVelocity < 0)
                    {
                        stateMachine.ChangeState(new FreeFallState());
                    }

                    break;
                case MoveState moveState:
                    if (!stateMachine.Target.tps.Grounded)
                    {
                        stateMachine.ChangeState(new FreeFallState());
                    }

                    break;
                case StandState standState:
                    if (!stateMachine.Target.tps.Grounded)
                    {
                        stateMachine.ChangeState(new FreeFallState());
                    }

                    break;
                case StunState stunState:
                    if (stunState.ElapsedTime > stunState.Duration)
                    {
                        stateMachine.ChangeState(new IdleState());
                    }

                    break;
            }
        }
    }
}