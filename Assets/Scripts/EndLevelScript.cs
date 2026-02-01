using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevelScript : MonoBehaviour
{


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Opzione 1: Vai subito

            if (other.GetComponent<PlayerController>().CheckChiave())
            {
                //Ti riporta al menu' principale
                Time.timeScale = 1f;
                SceneManager.LoadScene("menu");

            }
            // Opzione 2: Mostra UI prima
            // nextLevelUI.SetActive(true);
            // Time.timeScale = 0f;
        }
    }
}