using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeView : ViewUI
{
    [SerializeField] private UIInputController _inputController;
    
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitToLobbyButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(ResumeClick);
        exitToLobbyButton.onClick.AddListener(ExitLobbyClick);
    }

    private void ExitLobbyClick()
    {
        _inputController.SetState(UIInputController.State.IDLE);
        SceneManager.LoadScene(0);
    }

    private void ResumeClick()
    {
        _inputController.SetState(UIInputController.State.IDLE);
    }
}
