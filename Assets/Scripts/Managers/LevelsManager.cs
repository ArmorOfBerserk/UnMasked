using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Necessario per usare le Liste

public class LevelsManager : MonoBehaviour
{
    public static LevelsManager Instance;

    [Header("Configurazione Livelli")]
    // Trascina qui i nomi delle scene nell'ordine corretto dall'Inspector
    public List<string> levelScenes = new List<string>();
    
    private int currentLevelIndex = 0;

    void Awake()
    {
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

    // Carica un livello in base alla sua posizione nella lista
    public void LoadLevelByIndex(int index)
    {
        if (index >= 0 && index < levelScenes.Count)
        {
            currentLevelIndex = index;
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("Indice livello non valido nella lista!");
        }
    }

    // Passa al livello successivo nella lista
    public void NextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex < levelScenes.Count)
        {
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            // Se abbiamo finito la lista, vai al Game Over o Menu
            Debug.Log("Ultimo livello completato!");
            SceneManager.LoadScene("GameOver");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        currentLevelIndex = 0;
        SceneManager.LoadScene("MainMenu");
    }
}