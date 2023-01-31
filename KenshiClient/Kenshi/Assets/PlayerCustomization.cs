using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] private CustomizationManager _customizationManager;
    [SerializeField] private CustomizationData customizationData;

    [SerializeField] private SerializedDictionary<ClothingPart, GameObject> itemsWorn = new SerializedDictionary<ClothingPart, GameObject>();

    public void SetCustomization(CustomizationData data)
    {
        this.customizationData = data;
        RefreshCustomizationVisuals(data);
    }

    public void SetCustomizationSlot(ClothingPart part, int itemId)
    {
        customizationData.clothes[part] = itemId;
        RefreshCustomizationVisuals(customizationData);
    }
    
    private void RefreshCustomizationVisuals(CustomizationData data)
    {
        Debug.Log("Refresh visuals");
        foreach (var item in itemsWorn.Values)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        
        itemsWorn.Clear();

        foreach (var item in customizationData.clothes)
        {
            var inst = WearItem(item.Value);
            itemsWorn[item.Key] = inst;
        }
    }

    public GameObject WearItem(int itemValue)
    {
        if (_customizationManager.GetItem(itemValue, out var i))
        {
            if (i.part != null)
            {
                var inst = EquipUtils.EquipItem(gameObject, i.part);
                return inst;
            }
        }
        
        return null;
    }
}
