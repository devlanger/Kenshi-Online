using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class GameModeScriptable : DataObject<int>
{
    public string name;
    public Sprite icon;
    [TextArea(10, 50)]
    public string description;
}
