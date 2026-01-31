using UnityEngine;

public class WorldInteractable : MonoBehaviour, IInteractable
{
    [Header("Configurazione Oggetto")]
    [SerializeField] private Sprite interactionSprite; // L'immagine da mostrare

    public void Interact()
    {
        if (InteractionUI.Instance.IsOpen)
        {
            InteractionUI.Instance.CloseWindow();
        }
        else
        {
            InteractionUI.Instance.OpenWindow(interactionSprite);
        }
    }
}