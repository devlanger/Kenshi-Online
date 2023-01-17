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
    }

    private void Update()
    {        
        localPlayer.Input.rightClick = false;
        localPlayer.Input.leftClick = false;
        localPlayer.Input.dashing = false;
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