using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MovePlayableAsset : PlayableAsset
{
    public MovePlayableBehaviour.Data data;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var p = ScriptPlayable<MovePlayableBehaviour>.Create(graph);
        p.GetBehaviour().owner = go;
        p.GetBehaviour().data = data;
        return p;
    }
}
