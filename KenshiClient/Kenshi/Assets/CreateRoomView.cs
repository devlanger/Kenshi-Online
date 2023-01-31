using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreateRoomView : ViewUI
{
    [SerializeField] private TMP_InputField nameField;
    
    public async void CreateGameClick()
    {
        if (string.IsNullOrEmpty(nameField.text))
        {
            return;
        }
        
        await ConnectionController.Instance.ExecuteCommand($"create_game {nameField.text}");
        Deactivate();
    }
}
