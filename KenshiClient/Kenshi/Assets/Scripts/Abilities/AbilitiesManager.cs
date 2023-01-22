using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AbilitiesManager : ScriptableObject
{
    public List<AbilityScriptable> abilities = new List<AbilityScriptable>();
}
