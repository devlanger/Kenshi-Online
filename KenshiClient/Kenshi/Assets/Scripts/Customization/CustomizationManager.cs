using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class CustomizationManager : SerializedScriptableObject
{
    public List<CustomizationItem> items = new List<CustomizationItem>();

    public bool GetItem(int itemValue, out CustomizationItem i)
    {
        var item = items.FirstOrDefault(i => i.id == itemValue);
        if (item)
        {
            i = item;
            return true;
        }

        i = null;
        return false;
    }
}
