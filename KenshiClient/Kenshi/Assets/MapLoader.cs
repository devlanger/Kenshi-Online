using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class MapLoader : MonoBehaviour
{
    public static string MapToBeLoaded = "Forest";

    private void Awake()
    {
        if (!ConnectionController.Instance)
        {
            LoadScene();
        }
    }

    public static void LoadScene()
    {
        var map = MapsController.Instance.manager.items.Find(m => m.mapName == MapToBeLoaded);
        try
        {
            SceneManager.LoadScene(map.sceneName, LoadSceneMode.Additive);
        }
        catch (Exception e)
        {
            Debug.LogError($"Cant load map {MapToBeLoaded}");
            throw;
        }
    }
}
