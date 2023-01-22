using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class ThrowPlayableBehaviour : PlayableBehaviour
{
    [System.Serializable]
    public class Data
    {
        public AbilityInfo info;
        public GameObject throwable;
    }

    public GameObject owner;
    public Data data;
    
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
            var abilityInfo = p.Input.abilityInfo;
            Debug.Log("Throw");
            GameObject inst = GameObject.Instantiate(data.throwable, abilityInfo.user.transform.position + (UnityEngine.Quaternion.LookRotation(abilityInfo.aimPoint) * new Vector3(0, 1, 1)),
                Quaternion.LookRotation(abilityInfo.aimPoint));

            inst.GetComponent<TriggerCollisionHandler>().owner = abilityInfo.user.gameObject;
        
            Vector3 dir = (abilityInfo.aimPoint - inst.transform.position).normalized * 10;
        
            inst.transform.rotation = Quaternion.LookRotation(dir);
            inst.GetComponent<Rigidbody>().velocity = dir;
        }
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        
    }
}
