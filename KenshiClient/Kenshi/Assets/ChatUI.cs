using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ContentList chatList;
    [SerializeField] private ChatLabelListItem chatLabel;

    private void Awake()
    {
        FindObjectOfType<ConnectionController>().OnMessageReceived += ChatReceived;
        
        sendButton.onClick.AddListener(SendMessageClick);
    }

    private void SendMessageClick()
    {
        FindObjectOfType<ConnectionController>().ExecuteCommand($"chat_msg {inputField.text}");
        inputField.text = "";
    }

    private void ChatReceived(string arg1, string arg2)
    {
        if (arg1 == "ShowChatMessage")
        {
            var item = chatList.SpawnItem(chatLabel);
            item.Fill(arg2);
        }
    }
}
