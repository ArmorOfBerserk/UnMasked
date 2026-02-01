using UnityEngine;
using UnityEngine.SceneManagement; // Necessario per cambiare scena
using System.Collections;

public class EndGameTrigger : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Il pannello che contiene l'immagine di vittoria e i bottoni")]
    public GameObject winPanel;

    [Header("Settings")]
    [Tooltip("Nome esatto della scena del Menu Principale")]
    public string mainMenuSceneName = "menu";
    
    [Tooltip("Tempo di attesa prima di mostrare il pannello (opzionale)")]
    public float delayBeforeShowUI = 1f;

    private bool levelCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Controlla se è il player e se il livello non è già finito
        if (collision.CompareTag("Player") && !levelCompleted)
        {
            FinishLevel();
        }
    }

    private void FinishLevel()
    {
        levelCompleted = true;
        Debug.Log("Livello Completato!");

        // 1. Ferma tutto (Opzionale: ferma il player, la camera, il tempo)
        if (AutoScrollCamera.Instance != null)
        {
            AutoScrollCamera.Instance.StopScrolling();
        }

        // Blocca il movimento del player (tramite evento o disattivando lo script)
        // EventMessageManager.StopUsingMask(); // Se vuoi bloccare input maschera
        // collision.GetComponent<PlayerMovementBis>().canMove = false; // Se vuoi bloccare movimento

        // 2. Avvia la sequenza di vittoria
        StartCoroutine(ShowWinSequence());
    }

    private IEnumerator ShowWinSequence()
    {
        // Aspetta un attimo per dare enfasi
        yield return new WaitForSeconds(delayBeforeShowUI);

        // 3. Mostra il pannello
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            // Ferma il tempo del gioco (così la fisica si blocca)
            Time.timeScale = 0f; 
        }
        else
        {
            Debug.LogWarning("WinPanel non assegnato! Torno subito al menu.");
            LoadMainMenu();
        }
    }

    // Questa funzione va collegata al bottone nel WinPanel
    public void LoadMainMenu()
    {
        // Riattiva il tempo prima di cambiare scena (altrimenti il menu sarà bloccato!)
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}