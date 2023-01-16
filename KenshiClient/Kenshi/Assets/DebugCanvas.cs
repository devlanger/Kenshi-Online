using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        FindObjectOfType<ConnectionController>().ExecuteCommand($"join_game 5001");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
