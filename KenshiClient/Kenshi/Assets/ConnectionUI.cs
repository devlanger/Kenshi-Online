using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
         ConnectionController.Instance.OnMessageReceived += InstanceOnOnMessageReceived;
         exitButton.onClick.AddListener(() =>
         {
             Application.Quit();
         });
    }

    private void OnDestroy()
    {
        if (ConnectionController.Instance != null)
        {
            ConnectionController.Instance.OnMessageReceived -= InstanceOnOnMessageReceived;
        }
    }

    private void InstanceOnOnMessageReceived(string arg1, string arg2)
    {
        if (arg1 == "ShowConnectionMessage")
        {
            errorText.text = arg2;
            loadingIndicator.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
        }
    }
}
