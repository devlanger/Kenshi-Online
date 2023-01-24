using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillbarItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    public int hotkeyId;
    
    public void Fill(AbilityScriptable ability)
    {
        if (ability == null)
        {
            return;
        }
        
        icon.enabled = ability.icon != null;
        icon.sprite = ability.icon;
    }
}
