using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesController : MonoBehaviour
{
    public static StatesController Instance;
    
    private void Awake()
    {
        Instance = this;
    }
}
