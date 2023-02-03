using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AbilityScriptable : DataObject<int>
{
    public enum Type
    {
        ability = 1,
        tool = 2,
    }

    public Type type = Type.ability;
    public bool enabledInBuild = true;
    public Sprite icon;
    public AbilityData Data;
    public bool canCastInAir = true;
}