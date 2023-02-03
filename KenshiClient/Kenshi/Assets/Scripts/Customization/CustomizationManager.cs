using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class CustomizationManager : ItemsManager<CustomizationItem>
{
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

public class ItemsManager<T> : SerializedScriptableObject where T : DataObject<int>
{
    public List<T> items = new List<T>();

    public bool GetItem(int id, out T ability)
    {
        var a = items.FirstOrDefault(ab => ab.id == id);
        if (a == null)
        {
            ability = null;
            return false;
        }

        ability = a;
        return true;
    }
    
    [Button]
    public void AssignIds()
    {
        int id = items.Select(i => i.id).Max();
        
        foreach (var item in items)
        {
            if (item.id == 0)
            {
                item.id = ++id;
            }
            #if UNITY_EDITOR
            EditorUtility.SetDirty(item);
            #endif
        }
        
        
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }
}

public class DataObject<T> : SerializedScriptableObject
{
    public int id;
}