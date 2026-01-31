using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{

    public int lifes;

    public bool GameOver;

    public AudioSource MusicAmbient;
    public AudioSource GameOverSound;

    public GameObject Player;

    public GameObject GameOverText;
    public GameObject FadeIn;

    public GameObject PauseMenu;

    public GameObject PlayerHurtDust;

    private bool HasPlayedGameOverSound = false;

    [Header("Lifes Elements")]
    public GameObject LifeIcon1;
    public GameObject LifeIcon2;
    public GameObject LifeIcon3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int musicStatus = PlayerPrefs.GetInt("MusicStatus", 1);
        if (musicStatus == 1)
        {
            MusicAmbient.Play();
        }
        else
        {
            MusicAmbient.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (lifes <= 0)
        {
            if (!HasPlayedGameOverSound)
            {
                MusicAmbient.Stop();
                GameOverSound.Play();
                HasPlayedGameOverSound = true;
            }
            Player.SetActive(false);
            FadeIn.SetActive(true);
            GameOverText.SetActive(true);
            GameOver = true;
            Invoke("ChangeScene", 10f);
        }

        if (lifes == 3)
        {
            LifeIcon1.SetActive(true);
            LifeIcon2.SetActive(true);
            LifeIcon3.SetActive(true);
        }
        else if (lifes == 2)
        {
            LifeIcon1.SetActive(true);
            LifeIcon2.SetActive(true);
            LifeIcon3.SetActive(false);
        }
        else if (lifes == 1)
        {
            LifeIcon1.SetActive(true);
            LifeIcon2.SetActive(false);
            LifeIcon3.SetActive(false);
        }
        else if (lifes <= 0)
        {
            LifeIcon1.SetActive(false);
            LifeIcon2.SetActive(false);
            LifeIcon3.SetActive(false);
        }
    }

    public void ChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void Hurt()
    {
        Instantiate(PlayerHurtDust, Player.transform.position, Quaternion.identity);
        lifes--;
    }
}
