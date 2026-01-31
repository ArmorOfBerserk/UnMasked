using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject options;
    public GameObject mainMenu;

    public Toggle Music;

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("level1");
    }

    public void OptionsMenu()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
    }

    public void ExitOptionsMenu()
    {
        mainMenu.SetActive(true);
        options.SetActive(false);
    }

    public void Update()
    {
        PlayerPrefs.SetInt("MusicStatus", Music.isOn ? 1 : 0);
        PlayerPrefs.Save(); // Forza il salvataggio su disco
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
