using System;
using DefaultNamespace;
using UnityEngine;

public class GameModeController : MonoBehaviour
{
    public DeathmatchMode Mode;
    
    public static GameModeController Instance { get; set; }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Mode = new DeathmatchMode();
        Mode.Initialize(this);
    }

    public void FinishGame()
    {
        GameServer.Instance.DisconnectAllPlayers();
    }
}
