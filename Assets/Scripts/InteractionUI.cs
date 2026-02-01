using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject windowPanel;
    [SerializeField] private Image contentImage;
    [SerializeField] private CanvasGroup canvasGroup; // Riferimento al nuovo componente

    [Header("Effetti")]
    [SerializeField] private GameObject blurVolume;
    [SerializeField] private float fadeDuration = 0.3f; // Durata animazione

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Assicuriamoci che sia tutto spento all'avvio
        if(windowPanel != null) windowPanel.SetActive(false);
        if(blurVolume != null) blurVolume.SetActive(false);
    }

    public bool IsOpen => windowPanel.activeSelf;

    public void OpenWindow(Sprite spriteToShow)
    {
        contentImage.sprite = spriteToShow;
        windowPanel.SetActive(true);

        // PAUSA E BLUR
        Time.timeScale = 0f; 
        if (blurVolume != null) blurVolume.SetActive(true);

        // AVVIA ANIMAZIONE FADE IN
        // Fermiamo eventuali animazioni precedenti per evitare conflitti
        StopAllCoroutines(); 
        StartCoroutine(AnimateAlpha(0f, 1f)); // Da 0 (invisibile) a 1 (visibile)
    }

    public void CloseWindow()
    {
        // RIPRESA GIOCO IMMEDIATA (Per reattività)
        Time.timeScale = 1f;
        if (blurVolume != null) blurVolume.SetActive(false);
        
        // Disattiviamo subito il pannello (senza fade out per rendere il gioco scattante)
        windowPanel.SetActive(false);
        
        // Se volessi un fade out, dovresti usare una coroutine qui, 
        // ma attenzione che il gioco riprenderebbe a muoversi mentre il menu scompare.
    }

    // Coroutine per l'animazione che funziona anche in pausa
    private IEnumerator AnimateAlpha(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            // IMPORTANTE: Usiamo unscaledDeltaTime perché timeScale è 0!
            elapsedTime += Time.unscaledDeltaTime; 
            
            // Calcola la percentuale di completamento (da 0 a 1)
            float percentage = elapsedTime / fadeDuration;
            
            // Applica il valore interpolato
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, percentage);

            yield return null; // Aspetta il prossimo frame
        }

        // Assicuriamoci che alla fine sia esattamente al valore target
        canvasGroup.alpha = endAlpha;
    }
}