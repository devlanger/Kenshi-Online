using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    private ConnectionController connectionController;

    [SerializeField] private TextMeshProUGUI nicknameLabel;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button refreshGameButton;
    
    [Header("Game Room List")]
    [SerializeField] private GameRoomListItem gameRoomListItemPrefab;
    [SerializeField] private ContentList roomsList;
    
    [Header("Players List")]
    [SerializeField] private OnlinePlayerListItem _playerListItem;
    [SerializeField] private ContentList playersList;
    
    [SerializeField] private Canvas connectingCanvas;
    
    void Awake()
    {
        connectingCanvas.enabled = true;
        
        connectionController = FindObjectOfType<ConnectionController>();
        connectionController.OnMessageReceived += ConnectionControllerOnOnMessageReceived;
        connectionController.OnLogged += ConnectionControllerOnOnLogged;
        
        joinGameButton.onClick.AddListener(JoinGameClick);
        exitGameButton.onClick.AddListener(ExitGameClick);
        refreshGameButton.onClick.AddListener(RefreshGameClick);
        
        connectionController.OnUsersUpdated += ConnectionControllerOnOnUsersUpdated;
    }

    private void OnEnable()
    {
        RefreshUsersLogged();
    }

    private void ConnectionControllerOnOnUsersUpdated()
    {
        RefreshUsersLogged();
    }

    private void OnDestroy()
    {
        connectionController = FindObjectOfType<ConnectionController>();

        if (connectionController != null)
        {
            connectionController.OnMessageReceived -= ConnectionControllerOnOnMessageReceived;
            connectionController.OnLogged -= ConnectionControllerOnOnLogged;
            connectionController.OnUsersUpdated -= ConnectionControllerOnOnUsersUpdated;
        }
    }

    private void ConnectionControllerOnOnLogged(ConnectionDto obj)
    {
        if (obj == null)
        {
            return;
        }
        
        nicknameLabel.SetText(obj.nickname);
        connectingCanvas.enabled = false;
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
                GameRoomDto dto = JsonConvert.DeserializeObject<GameRoomDto>(arg2);
                GameRoomNetworkController.Port = ushort.Parse(dto.port);
                
                FindObjectOfType<GameRoomLobby>().Fill(dto);
                FindObjectOfType<GameRoomLobby>().Activate();
                break;
            case "JoinGameInstance":
                GameRoomDto dtoInstance = JsonConvert.DeserializeObject<GameRoomDto>(arg2);
                GameRoomNetworkController.Port = ushort.Parse(dtoInstance.port);
                
                FindObjectOfType<GameRoomLobby>().Fill(dtoInstance);
                FindObjectOfType<GameRoomLobby>().LoadGame();
                break;
        }
    }
    
    private void RefreshUsersLogged()
    {
        playersList.Clear();
        foreach (var item in ConnectionController.Instance.LoggedUsers)
        {
            var inst = playersList.SpawnItem(_playerListItem);
            inst.Fill(item);
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

    private async void Start()
    {
        ConnectionControllerOnOnLogged(ConnectionController.Instance.connectionDto);

        await connectionController.ExecuteCommand("games");
    }
}
