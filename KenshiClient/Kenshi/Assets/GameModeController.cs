using System;
using DefaultNamespace;
using UnityEngine;

public class GameModeController : MonoBehaviour
{
    private void Start()
    {
        GameMode mode = new DeathmatchMode();
        mode.Initialize(this);
    }

    public void FinishGame()
    {
        GameServer.Instance.DisconnectAllPlayers();
    }
}
