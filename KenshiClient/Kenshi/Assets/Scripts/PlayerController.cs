using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;

// [System.Serializable]
// public class DashState
// {
//     public KeyCode lastKey;
//     public float keyTime;
//
//     public Dictionary<KeyCode, MovementState> keys = new Dictionary<KeyCode, MovementState>()
//     {
//         {KeyCode.A, MovementState.DASH_LEFT}, {KeyCode.W, MovementState.DASH_FORWARD}, { KeyCode.S, MovementState.DASH_BACK }, {KeyCode.D, MovementState.DASH_RIGHT}
//     };
//     
//     public bool IsDashing => keys.Keys.Contains(lastKey) && Time.time < keyTime + 0.2f;
// }

public class PlayerController : MonoBehaviour
{
    public Player localPlayer;
    
    private void Awake()
    {
        localPlayer.gameObject.layer = 10;
        localPlayer.IsLocalPlayer = true;
        localPlayer.Interpolation.enabled = false;
    }
    

    private void Update()
    {
        if (localPlayer.Input.InputDirection != Vector3.zero)
        {
            var inputDirection = localPlayer.Input.InputDirection;
            float r = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
         
            localPlayer.Input.RotationY = r;   
        }

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        localPlayer.Input.CameraForward = forward;
        
        Physics.Raycast(Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition), out RaycastHit hit, 100, localPlayer.playerStateMachine.Variables._aimLayerMask);
        localPlayer.Input.AimDirection =
            hit.collider == null ? Camera.main.transform.forward * 100 : hit.point;
    }


    // private void UpdateDashState()
    // {
    //     foreach (var key in dashState.keys)
    //     {
    //         if (UnityEngine.Input.GetKeyDown(key.Key))
    //         {
    //             if (dashState.IsDashing)
    //             {
    //                 dashState.lastKey = KeyCode.None;
    //                 localPlayer.Input.dashing = true;
    //                 //movementStateMachine.ChangeState(key.Value);
    //             }
    //             
    //             dashState.lastKey = key.Key;
    //             dashState.keyTime = Time.time;
    //         }
    //     }
    // }
}