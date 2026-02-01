using System.Collections.Generic;
using UnityEngine;

public class MusicPlaylist : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource musicSource;

    [Header("Playlist")]
    [SerializeField] private List<AudioClip> tracks = new();
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool shuffle = false;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    private int index = 0;

    void Start()
    {
        if (tracks.Count == 0 || musicSource == null) return;

        musicSource.volume = musicVolume;

        if (shuffle) Shuffle(tracks);

        PlayIndex(0);
    }

    void Update()
    {
        if (musicSource.clip == null) return;

        if (!musicSource.isPlaying && musicSource.time == 0f)
        {
            Next();
        }
    }

    private void PlayIndex(int i)
    {
        musicSource.clip = tracks[i];
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void SetVolume(float value01)
    {
        musicVolume = Mathf.Clamp01(value01);
        musicSource.volume = musicVolume;
    }

    public void Next()
    {
        index++;
        if (index >= tracks.Count)
        {
            if (!loopPlaylist) return;
            index = 0;
            if (shuffle) Shuffle(tracks);
        }
        PlayIndex(index);
    }

    private static void Shuffle(List<AudioClip> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
