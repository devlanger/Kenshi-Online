using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInitializer : MonoBehaviour
{
    public GameServer server;
    
    private void Start()
    {
        Initialize();        
        server.StartServer();
    }

    private void Initialize()
    {
        GameServer.Config c = new GameServer.Config()
        {
            jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "testtesttest-123123123",
            containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "test",
            port = ushort.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001"),
            redis = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis",
        };

        #if UNITY_EDITOR
        c.redis = "localhost";
        #endif
        
        GameServer.Configuration = c;
    }
}
