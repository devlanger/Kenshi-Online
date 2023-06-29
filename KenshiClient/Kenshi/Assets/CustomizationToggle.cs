using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationToggle : MonoBehaviour
{
    [SerializeField] private Image icon;

    public CustomizationItem Data { get; private set; }
    
    public void SetData(CustomizationItem item)
    {
        this.Data = item;
        if (item.icon == null)
        {
            icon.enabled = false;
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;
    }
}
