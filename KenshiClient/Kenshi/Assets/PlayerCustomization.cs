using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] private CustomizationData customizationData;

    private Dictionary<ClothingPart, GameObject> itemsWorn = new Dictionary<ClothingPart, GameObject>();

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
        return null;
    }
}
