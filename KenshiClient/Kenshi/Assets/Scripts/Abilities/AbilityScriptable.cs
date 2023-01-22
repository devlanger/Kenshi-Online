using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AbilityScriptable : ScriptableDataObject
{
    public Sprite icon;
    public AbilityData Data;
}

public abstract class ScriptableDataObject : ScriptableObject
{
    public int Id;
} 