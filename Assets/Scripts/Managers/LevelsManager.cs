using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class LevelsManager : MonoBehaviour
{
    public static LevelsManager Instance;

    [Header("Punti Iniziali dei Livelli")]
    public List<Transform> levelStartPositions = new List<Transform>();
    private int currentLevelIndex = 0;
    private Transform playerTransform;
    [SerializeField] CinemachineCamera mainCamera;
    private float addLevelFloat = 0.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Trova il player nella scena
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Posiziona il player al primo livello
        LoadLevelByIndex(currentLevelIndex);
    }

    // Carica un "livello" spostando il player
    public void LoadLevelByIndex(int index)
    {
        if (index >= 0 && index < levelStartPositions.Count)
        {
            currentLevelIndex = index;
            if (playerTransform != null)
            {
                playerTransform.position = levelStartPositions[index].position;
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + addLevelFloat, mainCamera.transform.position.y, mainCamera.transform.position.z);
                addLevelFloat += 30.0f;
            }
            else
            {
                Debug.LogError("Player non trovato in scena! Assicurati che abbia il tag 'Player'.");
            }
        }
        else
        {
            Debug.LogError("Indice livello non valido nella lista!");
        }
    }

    // Passa al livello successivo nella scena
    public void NextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levelStartPositions.Count)
        {
            LoadLevelByIndex(currentLevelIndex);
        }
        else
        {
            Debug.Log("Ultimo livello completato!");
            // Puoi aggiungere qui un comportamento tipo "fine gioco" senza cambiare scena
        }
    }

    public void RestartLevel()
    {
        // Ricarica lo stesso livello (reset posizione)
        LoadLevelByIndex(currentLevelIndex);
    }

    public void GoToMenu()
    {
        // Se vuoi simulare menu, puoi riposizionare il player a un punto "menu" o fare un comportamento custom
        Debug.Log("Tornare al menu (implementa comportamento custom qui)");
    }
}
