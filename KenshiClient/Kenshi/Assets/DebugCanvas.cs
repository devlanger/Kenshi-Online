using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
    [SerializeField] private Button connectButton;

    void Awake()
    {
        #if UNITY_EDITOR
        GetComponent<Canvas>().enabled = true;
        #endif
        connectButton.onClick.AddListener(Click);
    }

    private void Click()
    {
        //SceneManager.LoadScene(1);
        FindObjectOfType<ConnectionController>().ExecuteCommand($"join_game gameroom-5001");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
