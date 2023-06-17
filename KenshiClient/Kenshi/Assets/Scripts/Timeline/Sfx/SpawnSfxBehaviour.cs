using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class SpawnSfxBehaviour : PlayableBehaviour
{
    [System.Serializable]
    public class Data
    {
        public AudioClip clip;
        public float volume = 0.5f;
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
            if (GameServer.IsServer) return;
            
            SfxController.Instance.PlaySound(data.clip, data.volume);
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
