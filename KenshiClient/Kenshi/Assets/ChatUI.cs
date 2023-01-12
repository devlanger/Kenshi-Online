using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChatController
{
    public ContentList chatList;
    public ChatLabelListItem chatLabel;
    public TMP_InputField inputField;

    public void Send(Action onSend)
    {
        onSend?.Invoke();
        inputField.text = "";
    }

    public void PushMessage(string message)
    {
        var item = chatList.SpawnItem(chatLabel);
        item.Fill(message);
    }
}

public class ChatUI : MonoBehaviour
{
    [SerializeField] private Button sendButton;
    [SerializeField] private ChatController chat;

    private void Awake()
    {
        FindObjectOfType<ConnectionController>().OnMessageReceived += ChatReceived;
        
        sendButton.onClick.AddListener(SendMessageClick);
        
        ChatReceived("ShowChatMessage", $"Welcome in Kenshi Online {Application.version} lobby!");
    }

    private void SendMessageClick()
    {
        chat.Send(() =>
        {
            FindObjectOfType<ConnectionController>().ExecuteCommand($"chat_msg {chat.inputField.text}");
        });
    }

    private void ChatReceived(string arg1, string arg2)
    {
        if (arg1 == "ShowChatMessage")
        {
            chat.PushMessage(arg2);
        }
    }
}
