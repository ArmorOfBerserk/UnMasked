using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
    public static LevelsManager Instance;
    
    [Header("Level Info")]
    public int currentLevel = 0;
    public int totalLevels = 5;
    
    void Awake()
    {
        // Singleton - sopravvive tra scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Carica livello specifico
    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        SceneManager.LoadScene("level" + levelIndex);
    }
    
    // Prossimo livello
    public void NextLevel()
    {
        currentLevel++;
        
        if (currentLevel > totalLevels)
        {
            // Hai vinto!
            SceneManager.LoadScene("GameOver");
        }
        else
        {
            SceneManager.LoadScene("level" + currentLevel);
        }
    }
    
    // Ricomincia livello corrente
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Torna al menu
    public void GoToMenu()
    {
        currentLevel = 0;
        SceneManager.LoadScene("MainMenu");
    }
}