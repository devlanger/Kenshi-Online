using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsListener : MonoBehaviour
{
    public AudioClip LandingAudioClip;        
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    public AudioClip[] FootstepAudioClips;
    public VfxScriptable footstepDirt;
    public void OnFootstep(AnimationEvent animationEvent)
    {
        if (GameServer.IsServer)
        {
            return;
        }
        
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
            }
        }

        VfxController.Instance.SpawnFx(footstepDirt, transform.position, Quaternion.identity);
    }

    public void OnLand(AnimationEvent animationEvent)
    {
        if (GameServer.IsServer)
        {
            return;
        }

        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
        }
    }
}
