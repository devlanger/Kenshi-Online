using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxController : Singleton<SfxController>
{
    public SfxManager manager;

    public void PlaySound(SfxId id, float volume = 1)
    {
        var clip = manager.GetItemBySfxId(id);
        if(clip != null)
        {
            SoundsManager.PlaySound(clip.clip, volume);
        }
    }
    
    public void PlaySound(SfxItem clip, float volume = 1)
    {
        SoundsManager.PlaySound(clip.clip, volume);
    }
    
    public void PlaySound(AudioClip clip, float volume = 1)
    {
        SoundsManager.PlaySound(clip, volume);
    }
}
