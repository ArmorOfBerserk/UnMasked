using System;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audio;
    [SerializeField] private AudioClip audioPassi;
    [SerializeField] private AudioClip audioSalto;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    public void PlayAudioPassi()
    {
        if (audio.isPlaying) return;
        
        audio.PlayOneShot(audioPassi);
    }
}
