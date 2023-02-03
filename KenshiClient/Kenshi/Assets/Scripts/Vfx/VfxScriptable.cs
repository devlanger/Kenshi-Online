using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VfxScriptable : DataObject<int>
{
    public GameObject vfx;
    public VfxController.VfxId vfxId;
}
