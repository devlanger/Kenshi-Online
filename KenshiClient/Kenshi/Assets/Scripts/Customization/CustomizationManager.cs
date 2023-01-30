using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class CustomizationManager : SerializedScriptableObject
{
    public List<CustomizationItem> items = new List<CustomizationItem>();
}
