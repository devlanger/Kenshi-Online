using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class MovePlayableBehaviour : PlayableBehaviour
{
    [System.Serializable]
    public class Data
    {
        public Vector3 velocity;
    }

    public Data data;
    public GameObject owner;
    
    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable)
    {
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (playable.GetPlayState() == PlayState.Playing)
        {
            var p = owner.GetComponent<Player>();
            if(!GameServer.IsServer && !p.IsLocalPlayer) p.GetComponent<Rigidbody>().isKinematic = false;

            p.tps.SetVelocity(p.transform.rotation * data.velocity);
        }
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        var p = owner.GetComponent<Player>();
        p.tps.SetVelocity(Vector3.zero);
        
        if(!GameServer.IsServer && !p.IsLocalPlayer) p.GetComponent<Rigidbody>().isKinematic = true;
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        
    }
}