using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;

public class Player : Mob
{
    public PlayerInterpolation Interpolation;
    public ThirdPersonController tps;
    
    public StarterAssetsInputs Input = new StarterAssetsInputs();
    public bool IsLocalPlayer { get; set; }
    public int NetworkId { get; set; }

    public PlayerStateMachine playerStateMachine;
    public PlayerStateMachine movementStateMachine;
    public Animator animator;

    public NetPeer peer;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStateMachine = new PlayerStateMachine();
        playerStateMachine.Target = this;
        playerStateMachine.Variables = GetComponent<StateMachineVariables>();
        playerStateMachine.ChangeState(new IdleState());
        
        movementStateMachine = new PlayerStateMachine();
        movementStateMachine.Target = this;
        movementStateMachine.Variables = GetComponent<StateMachineVariables>();
        movementStateMachine.ChangeState(new StandState());
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            movementStateMachine.CurrentState?.UpdateInput(movementStateMachine);
            playerStateMachine.CurrentState?.UpdateInput(playerStateMachine);
        }
        
        playerStateMachine.CurrentState?.FixedUpdate(playerStateMachine);
        playerStateMachine.CurrentState?.Update(playerStateMachine);
        
        movementStateMachine.CurrentState?.FixedUpdate(movementStateMachine);
        movementStateMachine.CurrentState?.Update(movementStateMachine);
        
        playerStateMachine.UpdateQueue();
        movementStateMachine.UpdateQueue();
        
        if (IsLocalPlayer)
        {
            Input.jump = false;
            Input.rightClick = false;
            Input.leftClick = false;
            Input.dashing = false;
        }
    }

    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            tps.CameraRotation();
        }
    }
}
