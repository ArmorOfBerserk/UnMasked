using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    private string classic_level = "level1";
    private string head_level = "headLevel";
    public void StartGame()
    {
        Debug.Log("ciaooo");
        SceneManager.LoadScene(classic_level);
    }
    public void GoToOptions()
    {
        SceneManager.LoadScene(head_level);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
