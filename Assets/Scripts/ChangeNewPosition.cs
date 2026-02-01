using UnityEngine;

public class ChangeNewPosition : MonoBehaviour
{
    // Viene chiamato quando qualcosa entra nel trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Controlliamo se è il Player a toccare l'oggetto
        if (other.CompareTag("Player"))
        {
            // 2. Prendiamo lo script PlayerMovement dal player
            PlayerMovement playerScript = other.GetComponent<PlayerMovement>();

            if (playerScript != null)
            {
                // 3. Aggiorniamo la start position usando la posizione di QUESTO checkpoint
                playerScript.SetNewStartPosition(transform.position);

                Debug.Log("Nuovo Checkpoint salvato: " + transform.position);
            }
        }
    }
}