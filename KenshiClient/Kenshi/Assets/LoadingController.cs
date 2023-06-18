using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : ViewUI
{
    public static LoadingController Instance;

    [SerializeField] private Slider slider;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Activate()
    {
        base.Activate();
        slider.value = 0;
    }

    public void SetProgress(float progess)
    {
        if (IsActive)
        {
            slider.value = progess;
        }
    }
}
