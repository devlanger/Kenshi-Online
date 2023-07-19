using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeInfoPanel : MonoBehaviour
{
    [SerializeField] private GameModesManager _modesManager;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image icon;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    private int modeIndex = 0;
    
    private void Awake()
    {
        nextButton.onClick.AddListener(Next);
        previousButton.onClick.AddListener(Previous);
        Fill(_modesManager.items[modeIndex]);
    }

    private void Previous()
    {
        modeIndex--;
        if (modeIndex < 0)
        {
            modeIndex = _modesManager.items.Count - 1;
        }
        Fill(_modesManager.items[modeIndex]);
    }

    private void Next()
    {
        modeIndex++;
        if (modeIndex == _modesManager.items.Count)
        {
            modeIndex = 0;
        }
        Fill(_modesManager.items[modeIndex]);
    }

    public void Fill(GameModeScriptable scriptable)
    {
        nameText.SetText(scriptable.name);
        descriptionText.SetText(scriptable.description);
        icon.sprite = scriptable.icon;
    }
}
