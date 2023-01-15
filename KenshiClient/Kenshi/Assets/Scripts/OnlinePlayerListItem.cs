using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlinePlayerListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    public void Fill(string item)
    {
        text.SetText(item);    
    }
}
