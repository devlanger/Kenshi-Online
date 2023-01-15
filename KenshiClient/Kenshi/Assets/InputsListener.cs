using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputsListener : MonoBehaviour
{
    [SerializeField] private Player localPlayer;

    private StarterAssetsInputs inputs => localPlayer.Input;
    
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
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

    public void OnJump(InputValue value)
    {
        inputs.JumpInput(value.isPressed);
    }

    public void OnMouseRight(InputValue value)
    {
        inputs.RightClickInput(value.isPressed);
    }
		
    public void OnMouseLeft(InputValue value)
    {
        inputs.LeftClickInput(value.isPressed);
    }
		
    public void OnTab(InputValue value)
    {
        TabInput(value.isPressed);
    }

    private void TabInput(bool valueIsPressed)
    {
        inputs.tab = valueIsPressed;
    }

    public void OnSprint(InputValue value)
    {
        inputs.SprintInput(value.isPressed);
    }
#endif
}
