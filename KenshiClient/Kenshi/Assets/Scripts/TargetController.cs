using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private Transform camTarget;
    [SerializeField] private Animator cameraControllerAnimator;
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private LayerMask charactersMask;

    private ThirdPersonController tpsController;
    
    private bool locked;
    public Player target;
    public event Action<Player> OnTargetSet;

    private void Awake()
    {
        tpsController = GameObject.FindObjectOfType<ThirdPersonController>();
        OnTargetSet += OnOnTargetSet;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            tpsController.CameraRotation();
        }
        else
        {
            Vector3 diff = target.transform.position - camTarget.transform.position;
            camTarget.transform.rotation = Quaternion.LookRotation(diff);             
        }
    }

    private void OnOnTargetSet(Player target)
    {
        this.target = target;
        cam.LookAt = target == null ? null : target.transform;

        if (target == null)
        {
            cameraControllerAnimator.Play("FreeCamera");
        }
        else
        {
            cameraControllerAnimator.Play("TargetCamera");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Physics.SphereCast(cam.transform.position, 5, cam.transform.forward, out var raycastHit, 25, charactersMask,
                    QueryTriggerInteraction.Ignore))
            {
                var c = raycastHit.collider;
                if (target == null)
                {
                    var p = c.GetComponent<Player>();
                    SetTarget(p != null ? p : null);
                }
                else
                {
                    SetTarget(null);
                }
            }
            else
            {
                SetTarget(null);
            }
        }
    }

    private void SetTarget(Player target)
    {
        this.target = target;
        OnTargetSet?.Invoke(target);
    }
}
