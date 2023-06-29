using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginMessagePanel : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    public Button cancelButton;

    private void Awake()
    {
        cancelButton.onClick.AddListener(CloseMessagePanel);
    }

    private void CloseMessagePanel()
    {
        messagePanel.SetActive(false);
    }

    public void SetMessage(string message)
    {
        messageText.SetText(message);
        messagePanel.SetActive(true);
    }
}
