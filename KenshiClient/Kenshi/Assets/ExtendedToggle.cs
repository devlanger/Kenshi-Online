using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExtendedToggle : ExtendedSelectable
{
    public UnityEvent OnToggleOn;
    public UnityEvent OnToggleOff;

    protected override void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(ToggleChanged);
    }

    private void ToggleChanged(bool arg0)
    {
        if(arg0)
        {
            OnToggleOn?.Invoke();
        }
        else
        {
            OnToggleOff?.Invoke();
        }
    }
}