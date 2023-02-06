using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoomLobby : ViewUI
{
    private Coroutine loadingCoroutine;

    [SerializeField] private Button enterButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private ContentList playersList;
    [SerializeField] private OnlinePlayerListItem playerItem;
    
    [SerializeField] private GameObject[] menuItems;

    public override void Activate()
    {
        base.Activate();
        foreach (var item in menuItems)
        {
            item.gameObject.SetActive(false);
        }
    }
    
    public override void Deactivate()
    {
        base.Deactivate();
        foreach (var item in menuItems)
        {
            item.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        enterButton.onClick.AddListener(Enter);
        leaveButton.onClick.AddListener(Leave);
    }

    public void Fill(GameRoomDto roomDto)
    {
        enterButton.enabled = roomDto.started;
        
        playersList.Clear();
        foreach (var item in roomDto.players)
        {
            var i = playersList.SpawnItem<OnlinePlayerListItem>(playerItem);
            i.Fill(item == roomDto.leaderUsername ? $"[Leader] {item}" : item);
        }

        var text = enterButton.GetComponentInChildren<TextMeshProUGUI>();
        if (ConnectionController.Instance.connectionDto.nickname == roomDto.leaderUsername)
        {
            if (!roomDto.started)
            {
                text.SetText("Start Game");
            }
            else
            {
                text.SetText("Join");
            }
        }
        else
        {
            if (roomDto.started)
            {
                text.SetText("Join");
            }
            else
            {
                text.SetText("Wait for leader...");
            }
        }
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
