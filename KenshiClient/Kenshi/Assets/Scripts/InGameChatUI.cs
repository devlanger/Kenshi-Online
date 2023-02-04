using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InGameChatUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup scrollBar;
    [SerializeField] private ChatController chat;

    public bool IsChatActive { get; set; }
    
    void Start()
    {
        IsChatActive = false;
        RefreshState();
        if(ConnectionController.Instance) ConnectionController.Instance.OnMessageReceived += ChatReceived;

        chat.PushMessage($"Welcome in Kenshi Online {Application.version} game room.");
        chat.inputField.onFocusSelectAll = true;
    }

    private void OnDestroy()
    {   
        var c = FindObjectOfType<ConnectionController>();

        if (c != null)
        {
            c.OnMessageReceived -= ChatReceived;
        }
    }

    public void SetState(bool active)
    {
        IsChatActive = active;
        RefreshState();
    }

    private void ChatReceived(string arg1, string arg2)
    {
        if (arg1 == "ShowChatMessage")
        {
            chat.PushMessage(arg2);
        }
    }
    
    public void SendChatMessage()
    {
        chat.Send(() =>
        {
            if (chat.inputField.text != "")
            {
                FindObjectOfType<ConnectionController>().ExecuteCommand($"chat_msg {chat.inputField.text}");
                //chat.PushMessage(chat.inputField.text);
            }
        });
    }
    
    private void RefreshState()
    {
        chat.inputField.gameObject.SetActive(IsChatActive);
        background.enabled = IsChatActive;
        scrollBar.alpha = IsChatActive ? 1 : 0;
        
        if (IsChatActive)
        {
            chat.inputField.Select();
            chat.inputField.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(chat.inputField.gameObject);
        }
    }
}
