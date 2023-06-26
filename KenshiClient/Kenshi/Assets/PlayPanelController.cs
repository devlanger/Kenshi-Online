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
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject matchmakingText;
    [SerializeField] private bool useTestServer = false;

    private bool isSearching = false;
    private ConnectionController _connectionController;
    private float waitingTime = 0;
    private Coroutine waitingCoroutine;

    private void Awake()
    {
        playButton.onClick.AddListener(PlayClick);
        SetState();
    }

    private void Start()
    {
        _connectionController = FindObjectOfType<ConnectionController>();
        _connectionController.OnMessageReceived += ConnectionControllerOnOnMessageReceived;
    }

    private void ConnectionControllerOnOnMessageReceived(string arg1, string arg2)
    {
        switch (arg1)
        {
            case "SetMatchmakingState":
                if (arg2 == "true")
                {
                    isSearching = true;
                    SetState();
                }
                else
                {
                    isSearching = false;
                    SetState();
                }
                
                if(waitingCoroutine != null) StopCoroutine(waitingCoroutine);
                waitingCoroutine = StartCoroutine(StartWaitingTimer());
                break;
        }
    }

    private IEnumerator StartWaitingTimer()
    {
        waitingTime = 0;
        while (isSearching)
        {
            TimeSpan time = TimeSpan.FromSeconds(waitingTime);
            string str = time .ToString(@"mm\:ss");
            timeText.SetText(str);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private async void PlayClick()
    {
        // isSearching = !isSearching;
        //
        // SetState();

        if (useTestServer)
        {
            ConnectionControllerOnOnMessageReceived("SetMatchmakingState", "true");
            FindObjectOfType<ConnectionController>().ExecuteCommand($"join_game gameroom-5001");
            return;
        }

        await _connectionController.ExecuteCommand(!isSearching ? $"start_matchmaking" : "stop_matchmaking");
    }

    private void SetState()
    {
        playButtonText.SetText(isSearching ? "Cancel" : "Play");
        matchmakingText.SetActive(isSearching);
    }

    private void Update()
    {
        UpdateWaitingTime();
    }

    private void UpdateWaitingTime()
    {
        if (!isSearching) return;
        waitingTime += Time.deltaTime;
    }
}
