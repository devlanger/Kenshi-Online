using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UIInputController : MonoBehaviour
{
    public enum State
    {
        IDLE = 0,
        WRITING_CHAT = 1,
        ESCAPE = 2,
        SCORES = 3,
    }

    [SerializeField] private PlayerInput _inputController;
    [SerializeField] private EscapeView _escapeView;
    [SerializeField] private InGameChatUI _chatView;
    [SerializeField] private ScoresView _scoresView;

    public State CurrentState = State.IDLE;

    private void Start()
    {
        _scoresView.Deactivate();
        SetState(State.IDLE);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetState(CurrentState == State.IDLE ? State.ESCAPE : State.IDLE);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (CurrentState == State.SCORES)
            {
                SetState(State.IDLE);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetState(CurrentState == State.IDLE ? State.SCORES : State.IDLE);
        }
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (CurrentState == State.WRITING_CHAT)
            {
                _chatView.SendChatMessage();
            }
            SetState(CurrentState == State.IDLE ? State.WRITING_CHAT : State.IDLE);
        }
    }

    public void SetState(State state)
    {
        switch (CurrentState)
        {
            case State.ESCAPE:
                _escapeView.Deactivate();
                break;
            case State.WRITING_CHAT:
                _chatView.SetState(false);
                break;
            case State.IDLE:
                Cursor.lockState = CursorLockMode.None;
                _inputController.enabled = false;
                break;
            case State.SCORES:
                _scoresView.Deactivate();
                break;
        }

        switch (state)
        {
            case State.IDLE:
                _inputController.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case State.ESCAPE:
                _escapeView.Activate();
                break;
            case State.WRITING_CHAT:
                _chatView.SetState(true);
                break;
            case State.SCORES:
                _scoresView.Activate();
                break;
        }
        
        CurrentState = state;
    }
}