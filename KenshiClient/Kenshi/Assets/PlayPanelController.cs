using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayPanelController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private GameObject matchmakingText;
    [SerializeField] private bool useTestServer = false;

    private bool isSearching = false;
    
    private void Awake()
    {
        playButton.onClick.AddListener(PlayClick);
        SetState();
    }

    private void PlayClick()
    {
        isSearching = !isSearching;

        SetState();

        if (useTestServer)
        {
            FindObjectOfType<ConnectionController>().ExecuteCommand($"join_game gameroom-5001");
        }
    }

    private void SetState()
    {
        playButtonText.SetText(isSearching ? "Cancel" : "Play");
        matchmakingText.SetActive(isSearching);
    }
}
