using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoomLobby : ViewUI
{
    private Coroutine loadingCoroutine;

    [SerializeField] private TMP_Text roomNumberLabel;
    [SerializeField] private TMP_Text roomNameLabel;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private ContentList playersList;
    [SerializeField] private OnlinePlayerListItem playerItem;
    [SerializeField] private TMP_Dropdown gameModeDropdown;
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private TMP_Dropdown playersDropdown;
    
    [SerializeField] private GameObject[] menuItems;

    private bool isRoomLeader = false;

    public RoomSettingsDto settingsDto = new RoomSettingsDto();

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
        
        mapsDropdown.ClearOptions();
        mapsDropdown.AddOptions(MapsController.Instance.manager.items.Select(m => m.mapName).ToList());

        gameModeDropdown.onValueChanged.AddListener((s) => RefreshSettings());
        mapsDropdown.onValueChanged.AddListener((s) => RefreshSettings());
        playersDropdown.onValueChanged.AddListener((s) => RefreshSettings());
        
        RefreshSettings();
        
        if (ConnectionController.Instance)
        {
            ConnectionController.Instance.OnMessageReceived += ConnectionControllerOnOnMessageReceived;
        }
    }

    private void OnDestroy()
    {
        if (ConnectionController.Instance)
        {
            ConnectionController.Instance.OnMessageReceived -= ConnectionControllerOnOnMessageReceived;
        }
    }

    private void ConnectionControllerOnOnMessageReceived(string arg1, string arg2)
    {
        switch (arg1)
        {
            case "change_room_settings":
                settingsDto = JsonConvert.DeserializeObject<RoomSettingsDto>(arg2);
                break;
        }
    }

    private void RefreshSettings(bool sendToServer = true)
    {
        settingsDto = new RoomSettingsDto()
        {
            mode = gameModeDropdown.options[gameModeDropdown.value].text,
            mapName = mapsDropdown.options[mapsDropdown.value].text,
            playersAmount = int.Parse(playersDropdown.options[playersDropdown.value].text)
        };

        if (sendToServer)
        {
            ChangeRoomSettings();
        }
    }

    private void RefreshSettingsVisuals()
    {
        mapsDropdown.SetValueWithoutNotify(string.IsNullOrEmpty(settingsDto.mapName) ? 0 : mapsDropdown.options.FindIndex(o => o.text == settingsDto.mapName));
        playersDropdown.SetValueWithoutNotify(playersDropdown.options.FindIndex(o => o.text == settingsDto.playersAmount.ToString()));
        gameModeDropdown.SetValueWithoutNotify(string.IsNullOrEmpty(settingsDto.mode) ? 0 : gameModeDropdown.options.FindIndex(o => o.text == settingsDto.mode));
    }

    public void Fill(GameRoomDto roomDto)
    {
        roomNumberLabel.text = roomDto.roomNumber;
        roomNameLabel.text = roomDto.displayName;
        enterButton.enabled = roomDto.started;
        
        playersList.Clear();
        foreach (var item in roomDto.players)
        {
            var i = playersList.SpawnItem<OnlinePlayerListItem>(playerItem);
            i.Fill(item == roomDto.leaderUsername ? $"[Leader] {item}" : item);
        }

        var text = enterButton.GetComponentInChildren<TextMeshProUGUI>();

        isRoomLeader = ConnectionController.Instance.connectionDto.nickname == roomDto.leaderUsername;
        
        mapsDropdown.enabled = isRoomLeader;
        gameModeDropdown.enabled = isRoomLeader;
        playersDropdown.enabled = isRoomLeader;
        
        if (isRoomLeader)
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

        settingsDto = roomDto.settings;
        RefreshSettingsVisuals();
    }

    public async void ChangeRoomSettings()
    {
        if (ConnectionController.Instance)
        {
            await ConnectionController.Instance.ExecuteCommandJson("ChangeRoomSettings",
                JsonConvert.SerializeObject(settingsDto));
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
