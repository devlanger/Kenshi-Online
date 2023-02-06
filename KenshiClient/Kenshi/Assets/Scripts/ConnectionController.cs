using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kenshi.Shared;
using Kenshi.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class ConnectionController : MonoBehaviour
{
    public HubConnection Connection { get; private set; }
    public ClientMessageHandler clientMessager { get; private set; }

    public event Action<string, string> OnMessageReceived;

    public static ConnectionController Instance;

    private string token;
    public bool useLocal = true;
    public string host = "127.0.0.1";
    public ConnectionDto connectionDto;
    public static string Nickname => Instance.connectionDto.nickname;
    public static string Token => Instance.connectionDto.token;
    public static string Host => Instance.useLocal ? "127.0.0.1" : Instance.host;
    public static string Ip => Instance.useLocal ? $"http://127.0.0.1:3330" : $"http://{Instance.host}:3330";

    public event Action<ConnectionDto> OnLogged;
    public event Action OnUsersUpdated;

    public List<string> LoggedUsers = new List<string>();

    private void Awake()
    {
#if !UNITY_EDITOR
        useLocal = false;
#endif
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Connection != null)
        {
            Connection.DisposeAsync();
        }
    }

    private IEnumerator Start()
    {
        bool connected = false;
        while (!connected)
        {
            Debug.Log("trying to connect");
            var t = Connect();
            yield return new WaitUntil(() => t.IsCompleted);

            if (Connection.State == HubConnectionState.Connected)
            {
                connected = true;
            }
            
            Debug.Log("Reconnecting in 5 sec...");
            yield return new WaitForSeconds(5);
        }
    }

    private async Task<bool> Connect()
    {
        try
        {
            Connection = new HubConnectionBuilder()
                .WithUrl(useLocal ? $"http://localhost:3330/gameHub" : $"http://{host}:3330/gameHub", opts =>
                {
                    opts.Headers["client_version"] = Application.version;
                })
                .Build();

            NetworkCommandProcessor.RegisterCommand("connect", (string[] param) => { SceneManager.LoadScene(1); });

            RegisterStringEventListener("UpdatePlayersList");
            RegisterStringEventListener("ListGameRooms");
            RegisterStringEventListener("JoinGameRoom");
            RegisterStringEventListener("JoinGameInstance");
            RegisterStringEventListener("ShowChatMessage");
            RegisterStringEventListener("ShowConnectionMessage");

            OnMessageReceived += OnOnMessageReceived;

            Connection.On<string>("SetConnectionData", (s =>
            {
                var data = JsonConvert.DeserializeObject<ConnectionDto>(s);
                connectionDto = data;
                OnLogged?.Invoke(data);
            }));

            clientMessager = new ClientMessageHandler(Connection);

            await Connection.StartAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    private void OnOnMessageReceived(string arg1, string arg2)
    {
        switch (arg1)
        {
            case "UpdatePlayersList":
                var dto = JsonConvert.DeserializeObject<List<string>>(arg2);
                LoggedUsers = dto;
                OnUsersUpdated?.Invoke();
                break;
        }
    }

    private void RegisterStringEventListener(string msg)
    {
        Connection.On<string>(msg, (s =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMessageReceived?.Invoke(msg, s);
            });
        }));
    }


    public async Task ExecuteCommand(string command)
    {
        await NetworkCommandProcessor.ProccessCommand(command, Connection);
    }

    public Task JoinGameRoom(string message)
    {
        Debug.Log(message);
        return Task.CompletedTask;
    }
}

public class ClientMessageHandler
{
    private readonly HubConnection _connection;

    public ClientMessageHandler(HubConnection connection)
    {
        _connection = connection;
    }

    public async Task CreateGame(string roomName)
    {
        await _connection.SendAsync("CreateGameRoom", "test-room");
    }
}