using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMenuToggle : MonoBehaviour
{
    [SerializeField] private Image icon;
    
    public void Fill(MapItem map)
    {
        icon.sprite = map.icon;
    }
}
