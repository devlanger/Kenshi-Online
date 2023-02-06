using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSounds : MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private void Awake()
    {
        if(TryGetComponent(out Button button))
        {
            button.onClick.AddListener(PlayAudio);
        }
    }

    public void PlayAudio()
    {
        SoundsManager.PlaySound(sound);
    }
}
