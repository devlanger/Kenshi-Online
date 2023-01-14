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

    private void Awake()
    {
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

    private async void Start()
    {
        Connection = new HubConnectionBuilder()
            .WithUrl($"{Ip}/gameHub")
            .Build();

        NetworkCommandProcessor.RegisterCommand("connect", (string[] param) =>
        {
            SceneManager.LoadScene(1);
        });

        Connection.On<string>("ListGameRooms", (s =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMessageReceived?.Invoke("ListGameRooms", s);
            });
        }));
        
        Connection.On<string>("JoinGameRoom", (s =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMessageReceived?.Invoke("JoinGameRoom", s);
            });
        }));
        
        Connection.On<string>("ShowChatMessage", (s =>
        {
            Console.WriteLine(s);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Console.WriteLine(s);

                OnMessageReceived?.Invoke("ShowChatMessage", s);
            });
        }));
        
        Connection.On<string>("SetConnectionData", (s =>
        {
            var data = JsonConvert.DeserializeObject<ConnectionDto>(s);
            connectionDto = data;
            OnLogged?.Invoke(data);
        }));
        
        clientMessager = new ClientMessageHandler(Connection); 

        await Connection.StartAsync();
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