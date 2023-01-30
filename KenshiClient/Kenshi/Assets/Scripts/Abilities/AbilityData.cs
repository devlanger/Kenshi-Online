using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public class AbilityData
{
    public string name;
    public float groundDuration;
    public float airDuration;
    public TimelineAsset behaviour;
    public ushort mana;
}
