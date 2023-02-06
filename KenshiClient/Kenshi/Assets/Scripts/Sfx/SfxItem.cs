using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SfxItem : DataObject<int>
{
    public SfxId sfxId;
    public AudioClip clip;
}


public enum SfxId
{
    hit_1 = 1,
    hit_2 = 2,
    hit_3 = 3,
    hit_4 = 4,
    hit_5 = 5,
    hit_6 = 6,
    hit_7 = 7,
    hit_8 = 8,
}