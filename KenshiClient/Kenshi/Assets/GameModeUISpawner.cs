using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeUISpawner : MonoBehaviour
{
    [SerializeField] private GameObject deathmatchCanvas;
    [SerializeField] private GameObject teamCanvas;

    private void Start()
    {
        var go = Instantiate(deathmatchCanvas, transform);
        go.SetActive(false);
    }
}
