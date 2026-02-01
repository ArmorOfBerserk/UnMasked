using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject fadeIn;

    private string classic_level = "level1";
    private string head_level = "headLevel";

    public GameObject Instructions;
    private bool instructions_active = false;
    public void StartGame()
    {
        Debug.Log("ciaooo");
    }
    public void GoToOptions()
    {
        SceneManager.LoadScene(head_level);
    }

    public void ToggleInstructions()
    {
        if (instructions_active)
        {
            instructions_active = false;
            Instructions.SetActive(false);
        }
        else
        {
            instructions_active = true;
            Instructions.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("level1");
    }

    public void GoToLevel()
    {
        Debug.Log("ciaooo");
        fadeIn.SetActive(true);
        Invoke("ChangeScene", 2f);
    }
}
