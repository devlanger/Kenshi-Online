using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachineVariables : MonoBehaviour
{
    public LayerMask _aimLayerMask;
    public Animator Animator;

    public int attackIndex = 0;
    public float lastAttackTime = 0;
}
