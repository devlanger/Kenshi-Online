using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsController : MonoBehaviour
{
    private SpawnPoint[] spawnPoints;
    public static SpawnPointsController Instance;

    private void Awake()
    {
        Instance = this;
        spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }
}
