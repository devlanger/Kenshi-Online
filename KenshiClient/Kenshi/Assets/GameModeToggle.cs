using System;
using DefaultNamespace;
using Kenshi.Shared.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameModeToggle : MonoBehaviour
{
    public Toggle toggle;
    public GameType GameType;

    public event Action<GameType> OnGameTypeChosen;
    public event Action<GameType> OnGameTypeUnchosen;
    public bool IsModeSelected => toggle.isOn; 

    private void Awake()
    {
        GetComponent<ExtendedToggle>().OnToggleOn.AddListener(OnGameModeChosen);
        GetComponent<ExtendedToggle>().OnToggleOff.AddListener(OnGameModeUnchosen);
    }

    private void OnGameModeUnchosen()
    {
        OnGameTypeUnchosen?.Invoke(GameType);
    }

    private void OnGameModeChosen()
    {
        OnGameTypeChosen?.Invoke(GameType);
    }
}
