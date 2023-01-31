using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomizationView : ViewUI
{
    [SerializeField] private PlayerCustomization character;
    [SerializeField] private CustomizationManager _customizationManager;

    public void WearItem(int itemId)
    {
        if (_customizationManager.GetItem(itemId, out var item))
        {
            character.SetCustomizationSlot(item.slot, item.id);
        }
    }
}
