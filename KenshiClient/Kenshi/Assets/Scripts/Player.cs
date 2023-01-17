using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;

public class Player : Mob
{
    public StarterAssetsInputs Input = new StarterAssetsInputs();
    public bool IsLocalPlayer { get; set; }

    public PlayerStateMachine playerStateMachine;
    public PlayerStateMachine movementStateMachine;
    
    private void Awake()
    {
        playerStateMachine = new PlayerStateMachine();
        playerStateMachine.Target = this;
        playerStateMachine.Variables = GetComponent<StateMachineVariables>();
        playerStateMachine.ChangeState(new IdleState());
        
        movementStateMachine = new PlayerStateMachine();
        movementStateMachine.ChangeState(new StandState());
        movementStateMachine.Variables = GetComponent<StateMachineVariables>();
        movementStateMachine.Target = this;
    }

    private void Update()
    {
        playerStateMachine.CurrentState?.Update(playerStateMachine);
        movementStateMachine.CurrentState?.Update(movementStateMachine);
    }
}
