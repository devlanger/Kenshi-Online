using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuSkillbarView : ViewUI
{
    [SerializeField] private AbilitiesManager manager;
    [SerializeField] private ContentList skillbookList;
    [SerializeField] private ContentList toolsList;
    [SerializeField] private SkillbarItem skillbookItem;
    [SerializeField] private ScrollRect _scrollRect;
    
    private void Start()
    {
        foreach (var ability in manager.items.Where(a => a.enabledInBuild))
        {
            var list = GetList(ability);

            var inst = list.SpawnItem(skillbookItem);
            inst.Fill(ability);
            
            var bt = inst.GetComponentInChildren<SkillbookDraggable>();
            bt.DragStart += (d) => { d.transform.parent = transform; };
            bt.abilityId = ability.id;
        }
        //_scrollRect.ScrollToBottom();
    }

    private ContentList GetList(AbilityScriptable ability)
    {
        var list = skillbookList;
        if (ability.type == AbilityScriptable.Type.tool)
        {
            list = toolsList;
        }

        return list;
    }
}

public static class ScrollRectExtensions
{
    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}