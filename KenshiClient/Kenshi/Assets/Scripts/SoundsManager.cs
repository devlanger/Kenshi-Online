using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static void PlaySound(AudioClip clip, float volume = 1f)
    {
        var go = new GameObject($"SoundClip_{clip.name}");

        var s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.volume = volume;
        s.PlayOneShot(clip);

        GameObject.Destroy(s, clip.length);
    }
}
