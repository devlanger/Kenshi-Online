using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class SkillbarView : SerializedMonoBehaviour
{
    public Dictionary<int, SkillbarItem> items = new Dictionary<int, SkillbarItem>();

    private void Awake()
    {
        foreach (var item in transform.GetComponentsInChildren<SkillbarItem>(true))
        {
            items[item.hotkeyId] = item;
        }

        if (AbilitiesController.Instance != null)
        {
            AbilitiesController.Instance.OnHotkeysChanged += InstanceOnOnHotkeysChanged;
            InstanceOnOnHotkeysChanged(AbilitiesController.Instance.hotkeys);
        }
    }

    private void OnDestroy()
    {
        if (AbilitiesController.Instance != null)
        {
            AbilitiesController.Instance.OnHotkeysChanged -= InstanceOnOnHotkeysChanged;
        }
    }

    private void InstanceOnOnHotkeysChanged(List<AbilitiesController.AbilityHotkey> obj)
    {
        foreach (var item in obj)
        {
            if (AbilitiesController.Instance.GetAbilityById(item.abilityId, out var a))
            {
                items[item.hotkeyId].Fill(a);
            }
        }
    }
}
