using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class CustomizationItem : SerializedScriptableObject
{
    public int id;
    public GameObject part;
    public ClothingPart slot;
}

[System.Serializable]
public class CustomizationData
{
    public Dictionary<ClothingPart, int> clothes = new Dictionary<ClothingPart, int>();
}

public enum ClothingPart
{
    head = 1,
    shirt = 2,
    pants = 3,
    boots = 4,
    gloves = 5,
    mask = 6,
}
