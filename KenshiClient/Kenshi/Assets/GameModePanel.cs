using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kenshi.Shared.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameModePanel : MonoBehaviour
{
    public List<GameModeToggle> toggles;

    public CanvasGroup canvasGroup;
    
    public List<GameType> SelectedTypes { get; private set; }

    public void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
    }
    
    private void Awake()
    {
        SelectedTypes = new List<GameType>();
        
        foreach (var item in toggles)
        {
            item.OnGameTypeChosen += (gt) =>
            {
                SelectedTypes.Add(gt);
            };
            
            item.OnGameTypeUnchosen += (gt) =>
            {
                if (SelectedTypes.Contains(gt))
                {
                    SelectedTypes.Remove(gt);
                }
            };

            if (item.IsModeSelected)
            {
                SelectedTypes.Add(item.GameType);
            }
        }
    }
}