using System;
using DefaultNamespace;
using UnityEngine;

public class GameModeController : MonoBehaviour
{
    public GameMode Mode;
    
    public static GameModeController Instance { get; set; }

    public event Action<GameMode> OnGameModeSet;

    public bool initialized = false;
    public GameType StartGameType;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameServer.IsServer)
        {
            if (Environment.GetEnvironmentVariable("GAME_MODE") != null)
            {
                int gameModeIndex = 0;
                if (int.TryParse(Environment.GetEnvironmentVariable("GAME_MODE"), out gameModeIndex))
                {
                    InitializeMode(GetModeInstance((GameType)gameModeIndex));
                }
            }
            else
            {
                InitializeMode(StartGameType);
            }
        }
    }

    private GameMode GetModeInstance(GameType gameModeType)
    {
        switch (gameModeType)
        {
            case GameType.DEATHMATCH:
                return new DeathmatchMode();
                break;
            case GameType.TEAM_DEATHMATCH:
                return new TeamDeathmatchMode();
        }

        return new DeathmatchMode();
    }
    
    public void InitializeMode(GameType gameModeType)
    {
        Mode = GetModeInstance(gameModeType);
        InitializeMode(Mode);
    }
    
    public void InitializeMode(GameMode mode)
    {
        if (initialized) return;
        
        Mode = mode;
        Mode.Initialize(this);
        SetGameMode(Mode);
        initialized = true;
    }

    public void FinishGame()
    {
        GameServer.Instance.DisconnectAllPlayers();
    }

    public void SetGameMode(GameMode mode)
    {
        OnGameModeSet?.Invoke(mode);
    }
}
