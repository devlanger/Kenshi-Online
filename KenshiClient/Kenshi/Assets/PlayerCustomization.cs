using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class PlayerCustomization : SerializedMonoBehaviour
{
    [SerializeField] private CustomizationManager _customizationManager;
    public CustomizationData customizationData;

    [SerializeField] private Dictionary<ClothingPart, GameObject> itemsWorn = new Dictionary<ClothingPart, GameObject>();

    public void SetCustomization(CustomizationData data)
    {
        this.customizationData = data;
        RefreshCustomizationVisuals(data);
    }

    public void SetCustomizationSlot(ClothingPart part, int itemId)
    {
        customizationData.items[part] = itemId;
        RefreshCustomizationVisuals(customizationData);
    }

    private void RefreshCustomizationVisuals(CustomizationData data)
    {
        var keys = new HashSet<ClothingPart>();
        foreach (var item in itemsWorn)
        {
            if (item.Value != null && customizationData.items.ContainsKey(item.Key))
            {
                Destroy(item.Value.gameObject);
                keys.Add(item.Key);
            }
        }

        foreach (var key in keys)
        {
            itemsWorn.Remove(key);
        }
        
        foreach (var item in customizationData.items)
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

    private CustomizationItem GetRandomItem(ClothingPart part)
    {
        var items = _customizationManager.GetAll(part);
        return items.Any() ? items[UnityEngine.Random.Range(0, items.Count)] : null;
    }
    
    public void Randomize()
    {
        var data = new CustomizationData()
        {
            items = new Dictionary<ClothingPart, int>()
        };
        
        foreach (ClothingPart slot in Enum.GetValues(typeof(ClothingPart)))
        {
            var c = GetRandomItem(slot);
            if (c != null)
            {
                data.items[slot] = c.id;
            }
        }
        
        SetCustomization(data);
    }
}
