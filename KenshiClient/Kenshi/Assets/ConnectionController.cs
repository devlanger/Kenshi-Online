using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kenshi.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class ConnectionController : MonoBehaviour
{
    public HubConnection Connection { get; private set; }
    public ClientMessageHandler clientMessager { get; private set; }

    public event Action<string, string> OnMessageReceived;

    public static ConnectionController Instance;
    
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

    private async void Start()
    {
        Connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:3330/gameHub")
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