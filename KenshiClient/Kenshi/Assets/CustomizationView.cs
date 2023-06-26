using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class CustomizationView : ViewUI
{
    [SerializeField] private PlayerCustomization character;
    [SerializeField] private CustomizationManager _customizationManager;

    private void Awake()
    {
        character.SetCustomization(PlayerController.GetCustomization());
    }

    public void WearItem(int itemId)
    {
        if (_customizationManager.GetItem(itemId, out var item))
        {
            character.SetCustomizationSlot(item.slot, item.id);
        
            PlayerPrefs.SetString("customization", JsonConvert.SerializeObject(character.customizationData));
        }
    }
}
