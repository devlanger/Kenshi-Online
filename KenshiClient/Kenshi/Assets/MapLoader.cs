using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class MapLoader : MonoBehaviour
{
    public static string MapToBeLoaded = "Map_1";

    private void Awake()
    {
        if (!ConnectionController.Instance)
        {
            LoadScene();
        }
    }

    public static void LoadScene()
    {
        SceneManager.LoadScene(MapToBeLoaded, LoadSceneMode.Additive);
    }
}
