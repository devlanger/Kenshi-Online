using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    [SerializeField] private SfxItem sfxId;
    [SerializeField] private float volume = 0.5f;

    public void Play()
    {
        if (GameServer.IsServer)
            return;
        
        SfxController.Instance.PlaySound(sfxId, volume);
    }
}
