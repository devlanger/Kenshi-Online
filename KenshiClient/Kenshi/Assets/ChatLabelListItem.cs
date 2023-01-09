using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatLabelListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void Fill(string s)
    {
        text.SetText(s);
    }
}
