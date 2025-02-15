using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachineVariables : MonoBehaviour
{
    public LayerMask _aimLayerMask;
    public LayerMask GroundLayers;
    public Animator Animator;

    public int attackIndex = 0;
    public float lastAttackTime = 0;
    public bool IsAttacking = false;
    public bool Grounded;
    public bool Jumping;
    public int jumpIndex = 0;
    public bool finishedAirCombo;
    public bool AlternateDashAnimation { get; set; }
}
