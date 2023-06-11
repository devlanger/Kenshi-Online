using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class SpawnPointsController : MonoBehaviour
{
    public SpawnPoint[] spawnPoints;
    public static SpawnPointsController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
    }
    
    public Vector3 GetRandomSpawnPoint()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }
}
