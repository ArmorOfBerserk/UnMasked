using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioPassi;
    [SerializeField] private AudioClip audioSalto;

    [SerializeField] private float volumePassi = 0.5f;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        audioSource.volume = volumePassi;
    }

    public void PlayAudioPassi()
    {
        if (audioSource.isPlaying) return;

        audioSource.PlayOneShot(audioPassi);
    }

    public void PlayAudioSalto()
    {
        audioSource.PlayOneShot(audioSalto);
    }
}
