using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapsMenuPanel : MonoBehaviour
{
    [SerializeField] private MapsManager _manager;
    [SerializeField] private ContentList list;
    [SerializeField] private MapMenuToggle prefab;

    private void Awake()
    {
        foreach (var map in _manager.items)
        {
            var inst = list.SpawnItem(prefab);
            inst.Fill(map);
            inst.GetComponent<Toggle>().isOn = true;
        }
    }
}
