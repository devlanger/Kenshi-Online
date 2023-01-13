using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    private ConnectionController connectionController;

    [SerializeField] private TextMeshProUGUI nicknameLabel;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button refreshGameButton;
    [SerializeField] private GameRoomListItem gameRoomListItemPrefab;
    [SerializeField] private ContentList roomsList;
    
    void Awake()
    {
        connectionController = FindObjectOfType<ConnectionController>();
        connectionController.OnMessageReceived += ConnectionControllerOnOnMessageReceived;
        connectionController.OnLogged += ConnectionControllerOnOnLogged;
        
        createGameButton.onClick.AddListener(CreateGameClick);
        joinGameButton.onClick.AddListener(JoinGameClick);
        exitGameButton.onClick.AddListener(ExitGameClick);
        refreshGameButton.onClick.AddListener(RefreshGameClick);
    }

    private void OnDestroy()
    {
        connectionController = FindObjectOfType<ConnectionController>();

        if (connectionController != null)
        {
            FindObjectOfType<ConnectionController>().OnMessageReceived -= ConnectionControllerOnOnMessageReceived;
            FindObjectOfType<ConnectionController>().OnLogged -= ConnectionControllerOnOnLogged;
        }
    }

    private void ConnectionControllerOnOnLogged(ConnectionDto obj)
    {
        nicknameLabel.SetText(obj.nickname);
    }

    private async void RefreshGameClick()
    {
        await connectionController.ExecuteCommand("games");
    }

    private void ExitGameClick()
    {
        Application.Quit();
    }

    private void ConnectionControllerOnOnMessageReceived(string arg1, string arg2)
    {
        switch (arg1)
        {
            case "ListGameRooms":
                roomsList.Clear();
                ContainerDto[] list = JsonConvert.DeserializeObject<ContainerDto[]>(arg2);
                foreach (var item in list)
                {
                    SpawnRoomListItem(item);
                }
                break;
            case "JoinGameRoom":
                string port = arg2;
                GameRoomNetworkController.Port = ushort.Parse(port);
                SceneManager.LoadScene(1);
                break;
        }
    }

    private void SpawnRoomListItem(ContainerDto item)
    { 
        var roomListItem = roomsList.SpawnItem(gameRoomListItemPrefab);
        roomListItem.Fill(item);
        
        roomListItem.GetComponent<ExtendedToggle>().OnToggleOn.AddListener((() =>
        {
            connectionController.ExecuteCommand($"join_game {item.Id}");
        }));
    }

    private async void JoinGameClick()
    {
        await connectionController.ExecuteCommand("join_game");
    }

    private async void CreateGameClick()
    {
        await connectionController.ExecuteCommand("create_game x");
    }

    private async void Start()
    {
        await connectionController.ExecuteCommand("games");
    }
}
