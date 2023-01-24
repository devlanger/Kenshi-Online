using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuSkillbarView : ViewUI
{
    [SerializeField] private AbilitiesManager manager;
    [SerializeField] private ContentList skillbookList;
    [SerializeField] private SkillbarItem skillbookItem;
    
    private void Start()
    {
        foreach (var ability in manager.abilities.Where(a => a.enabledInBuild))
        {
            var inst = skillbookList.SpawnItem(skillbookItem);
            inst.Fill(ability);
            
            var bt =inst.GetComponentInChildren<SkillbookDraggable>();
            bt.DragStart += (d) => { d.transform.parent = transform; };
            bt.abilityId = ability.Id;
        }
    }
}
