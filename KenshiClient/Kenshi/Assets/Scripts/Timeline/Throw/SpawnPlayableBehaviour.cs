using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class SpawnPlayableBehaviour : PlayableBehaviour
{
    [System.Serializable]
    public class Data
    {
        public enum SpawnType
        {
            THROW = 1,
            AT_HIT_POINT = 2,
            AT_USER = 3,
        }

        public SpawnType spawnType = SpawnType.THROW;
        public Vector3 spawnOffset;
        public AbilityInfo info;
        public GameObject throwable;
        public float speed = 20;
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
            switch (data.spawnType)
            {
                case Data.SpawnType.THROW:
                    GameObject inst = GameObject.Instantiate(data.throwable, abilityInfo.user.transform.position + (UnityEngine.Quaternion.LookRotation(abilityInfo.hitPoint) * new Vector3(0, 1, 1)),
                        Quaternion.LookRotation(abilityInfo.hitPoint));

                    inst.GetComponent<TriggerCollisionHandler>().owner = abilityInfo.user;
        
                    Vector3 dir = (abilityInfo.hitPoint - inst.transform.position).normalized * data.speed;
        
                    inst.transform.rotation = Quaternion.LookRotation(dir);
                    inst.GetComponent<Rigidbody>().velocity = dir;
                    break;
                
                case Data.SpawnType.AT_USER:
                    GameObject inst2 = GameObject.Instantiate(data.throwable, abilityInfo.user.transform.position + (abilityInfo.user.transform.rotation * data.spawnOffset), abilityInfo.user.transform.rotation);
                    inst2.GetComponent<TriggerCollisionHandler>().owner = abilityInfo.user;
                    break;
                
                case Data.SpawnType.AT_HIT_POINT:
                    GameObject inst3 = GameObject.Instantiate(data.throwable, abilityInfo.hitPoint + (abilityInfo.user.transform.rotation * data.spawnOffset), Quaternion.identity);
                    inst3.GetComponent<TriggerCollisionHandler>().owner = abilityInfo.user;
                    break;
            }
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
