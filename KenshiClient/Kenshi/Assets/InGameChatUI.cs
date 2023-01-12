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
    
    // Start is called before the first frame update
    void Start()
    {
        IsChatActive = false;
        RefreshState();
        
        chat.PushMessage($"Welcome in Kenshi Online {Application.version} game room.");
        chat.inputField.onFocusSelectAll = true;
    }

    public void SetState(bool active)
    {
        IsChatActive = active;
        RefreshState();
    }

    public void SendChatMessage()
    {
        chat.Send(() =>
        {
            if (chat.inputField.text != "")
            {
                chat.PushMessage(chat.inputField.text);
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
