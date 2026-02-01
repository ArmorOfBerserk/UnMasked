using System;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audio;
    [SerializeField] private AudioClip audioPassi;
    [SerializeField] private AudioClip audioSalto;

    [SerializeField] private float volumePassi = 0.5f;


    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        audio.volume = volumePassi;
    }

    public void PlayAudioPassi()
    {
        if (audio.isPlaying) return;

        audio.PlayOneShot(audioPassi);
    }

    public void PlayAudioSalto()
    {
        audio.PlayOneShot(audioSalto);
    }
}
