using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "LoginScene")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
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
