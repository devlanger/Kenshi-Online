using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillbarItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image icon;
    public int hotkeyId;
    public AbilityScriptable.Type type;
    
    public void Fill(AbilityScriptable ability)
    {
        if (ability == null)
        {
            return;
        }

        nameText?.SetText(ability.name);
        descriptionText?.SetText(ability.description);
        icon.enabled = ability.icon != null;
        icon.sprite = ability.icon;
    }
}
