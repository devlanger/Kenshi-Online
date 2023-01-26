using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesController : MonoBehaviour
{
    public static StatesController Instance;
    
    public GameObject manaLoadEffect;

    private void Awake()
    {
        Instance = this;
    }
}
