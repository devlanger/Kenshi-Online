using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoomLobby : ViewUI
{
    private Coroutine loadingCoroutine;

    [SerializeField] private Button enterButton;
    [SerializeField] private Button leaveButton;

    private void Awake()
    {
        enterButton.onClick.AddListener(Enter);
        leaveButton.onClick.AddListener(Leave);
    }

    public void Fill(GameRoomDto roomDto)
    {
        enterButton.enabled = roomDto.started;
    }
    
    public async void Enter()
    {
        await ConnectionController.Instance.ExecuteCommand("join_game_instance");
    }
    
    public async void Leave()
    {
        Deactivate();
        await ConnectionController.Instance.ExecuteCommand("leave_game");
    }
    
    public void LoadGame()
    {
        if (loadingCoroutine == null)
        {
            loadingCoroutine = ConnectionController.Instance.StartCoroutine(LoadYourAsyncScene());
        }
    }

    IEnumerator LoadYourAsyncScene()
    {
        LoadingController.Instance.Activate();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
        {
            yield return 0;
        }

        Debug.Log("Loaded");
        yield return new WaitUntil(() => GameRoomNetworkController.Instance.Connected);
        LoadingController.Instance.Deactivate();
        Debug.Log("Deactiveeeeeee");
    }
}
