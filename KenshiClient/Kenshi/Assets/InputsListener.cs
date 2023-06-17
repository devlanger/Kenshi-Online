using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Enums;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputsListener : MonoBehaviour
{
    [SerializeField] private Player localPlayer;
    [SerializeField] private NinjaPlayerinputs input;

    private StarterAssetsInputs inputs => localPlayer.Input;

    private void Awake()
    {
        input = new NinjaPlayerinputs();
        input.Enable();
    }

    private void Update()
    {
        inputs.leftClick = this.input.Player.MouseLeft.IsPressed();
        inputs.rightClick = this.input.Player.MouseRight.IsPressed();
        inputs.sprint = this.input.Player.Sprint.IsPressed();
        inputs.tab = this.input.Player.Tab.IsPressed();
    }

    public void OnJump(InputValue value)
    {
        switch (localPlayer.movementStateMachine.CurrentState)
        {
            case FreeFallState state:
            case JumpState jumpStates:
                if (localPlayer.movementStateMachine.Variables.jumpIndex < 2)
                {
                    inputs.jump = value.isPressed;
                }
                break;
            default:
                inputs.jump = value.isPressed;
                break;
        }
    }

    public void OnMove(InputValue value)
    {
        inputs.MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if(inputs.cursorInputForLook)
        {
            inputs.LookInput(value.Get<Vector2>());
        }
    }
}
