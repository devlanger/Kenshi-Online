using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Kenshi.Shared.Enums;
using Newtonsoft.Json;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class DashInput
{
    public DashState.Data.DashIndex dashIndex;
    public KeyCode lastKey;
    public float keyTime;

    public Dictionary<KeyCode, DashState.Data.DashIndex> keys = new Dictionary<KeyCode, DashState.Data.DashIndex>()
    {
        {KeyCode.A, DashState.Data.DashIndex.left}, {KeyCode.W, DashState.Data.DashIndex.forward}, { KeyCode.S, DashState.Data.DashIndex.back }, {KeyCode.D, DashState.Data.DashIndex.right}
    };
    
    public bool IsDashing => keys.Keys.Contains(lastKey) && Time.time < keyTime + 0.2f;
}

public class PlayerController : MonoBehaviour
{
    public Player localPlayer;

    [SerializeField] private DashInput dashInput;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    
    private void Awake()
    {
        dashInput = new DashInput();
        localPlayer.gameObject.layer = 10;
        localPlayer.IsLocalPlayer = true;
        localPlayer.Interpolation.enabled = false;
        
        localPlayer.GetComponentInChildren<PlayerCustomization>().SetCustomization(GetCustomization());
    }

    public static CustomizationData GetCustomization()
    {
        if (PlayerPrefs.HasKey("customization"))
        {
            return JsonConvert.DeserializeObject<CustomizationData>(PlayerPrefs.GetString("customization"));
        }

        return new CustomizationData();
    }

    private void Update()
    {
        UpdateDashState();
        UpdateCam();
    }
    private void UpdateCam()
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
        localPlayer.Input.HitPoint =
            hit.collider == null ? Camera.main.transform.forward * 100 : hit.point;

        switch (localPlayer._movementStateManagement.StateId)
        {
            case FSMStateId.move:
            case FSMStateId.jump:
            case FSMStateId.freefall:
                _virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(_virtualCamera.m_Lens.FieldOfView, localPlayer.Input.sprint ? 75 : 60, 0.05f);
                break;
            default:
                _virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(_virtualCamera.m_Lens.FieldOfView, 60, 0.05f);
                break;
        }
    }


    private void UpdateDashState()
    {
        foreach (var key in dashInput.keys)
        {
            if (UnityEngine.Input.GetKeyDown(key.Key))
            {
                if (dashInput.lastKey == key.Key && Time.time < dashInput.keyTime + 0.2f)
                {
                    dashInput.lastKey = KeyCode.None;
                    localPlayer.Input.dashing = true;
                    localPlayer.Input.dashIndex = key.Value;
                }
                else
                {
                    dashInput.lastKey = key.Key;
                    dashInput.keyTime = Time.time;
                }
            }
        }
    }
}