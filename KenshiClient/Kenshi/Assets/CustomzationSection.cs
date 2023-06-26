using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CustomzationSection : SerializedMonoBehaviour
{
    [SerializeField] private CustomizationView _customizationView;
    [SerializeField] private CustomizationManager _customizationManager;
    [SerializeField] private ClothingPart part;
    [SerializeField] private ContentList list;
    [SerializeField] private ExtendedToggle itemPrefab;
    [SerializeField] private ToggleGroup _group;
    
    private void Awake()
    {
        var l = new List<Tuple<ExtendedToggle, int>>();
        Toggle selected = null;
        foreach (var item in _customizationManager.items.Where(i => i.slot == part).ToList())
        {
            var inst = list.SpawnItem<ExtendedToggle>(itemPrefab);
            
            if (inst.TryGetComponent(out Toggle toggle))
            {
                toggle.group = _group;
                if (selected == null && PlayerController.GetCustomization().items.TryGetValue(part, out int id))
                {
                    if (id == item.id)
                    {
                        selected = toggle;
                    }
                }
            }
            l.Add(new (inst, item.id));
        }
        
        if(selected) selected.isOn = true;

        foreach (var item in l)
        {
            item.Item1.GetComponent<ExtendedToggle>().OnToggleOn.AddListener(() =>
            {
                _customizationView.WearItem(item.Item2);
            });
        }
    }
}
