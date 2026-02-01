using UnityEngine;

public class EndLevelScript : MonoBehaviour
{
    // public GameObject nextLevelUI; // Opzionale: pannello "Livello Completato"

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Opzione 1: Vai subito

            if (other.GetComponent<PlayerController>().CheckChiave())
            {
                TransitionManager.Instance.TransitionToLevel("NextLevel");

            }
            // Opzione 2: Mostra UI prima
            // nextLevelUI.SetActive(true);
            // Time.timeScale = 0f;
        }
    }
}