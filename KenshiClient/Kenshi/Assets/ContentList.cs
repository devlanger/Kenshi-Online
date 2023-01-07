using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentList : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private bool removeChildren = true;
    private List<GameObject> list = new List<GameObject>();

    private void Awake()
    {
        if (removeChildren)
        {
            foreach (Transform item in parent.transform)
            {
                Destroy(item.gameObject);
            }
        }
    }

    public T SpawnItem<T>(T item) where T : MonoBehaviour
    {
        var inst = Instantiate(item, parent);
        list.Add(inst.gameObject);
        return inst.GetComponent<T>();
    }

    public void Clear()
    {
        foreach (var item in list)
        {
            Destroy(item.gameObject);
        }
        
        list.Clear();
    }
}
