using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MapItem : DataObject<int>
{
    public string mapName;
    public string sceneName;
    public Material skybox;
}
