using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

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
    public SerializedDictionary<ClothingPart, int> clothes = new SerializedDictionary<ClothingPart, int>();
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
