using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameModeUISpawner : MonoBehaviour
{
    [SerializeField] private GameObject deathmatchCanvas;
    [SerializeField] private GameObject teamDeathmatchCanvas;

    private GameObject modeCanvas;

    public event Action<ScoresView> OnScoreViewSpawn;
    
    private void Start()
    {
        var gameModeController = FindObjectOfType<GameModeController>();
        gameModeController.OnGameModeSet += SpawnModeCanvas;
    }

    private void SpawnModeCanvas(GameMode mode)
    {
        if(modeCanvas != null) Destroy(modeCanvas.gameObject);

        var canvasPrefab = GetModeCanvas(mode);
        if (canvasPrefab != null)
        {
            var go = Instantiate(canvasPrefab, transform);
            OnScoreViewSpawn?.Invoke(FindObjectOfType<ScoresView>(true));
        }
    }

    private GameObject GetModeCanvas(GameMode mode)
    {
        switch (mode)
        {
            case DeathmatchMode dmMode:
                return deathmatchCanvas;
            case TeamDeathmatchMode tmMode:
                return teamDeathmatchCanvas;
        }

        return null;
    }
}
