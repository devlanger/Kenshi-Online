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
        var item = _customizationManager.items.FirstOrDefault(i => i.id == itemId);
        if (item == null)
        {
            return;
        }

        character.SetCustomizationSlot(item.slot, itemId);
    }
}
