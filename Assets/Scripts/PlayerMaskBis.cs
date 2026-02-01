using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMaskBis : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mask;
    [SerializeField] private Slider maskSlider;
    [SerializeField] private GameObject dxEye;
    [SerializeField] private GameObject sxEye;

    [Header("Blinking Settings")]
    public float blinkDuration = 0.1f;  // Durata totale dello sbatte
    public float blinkInterval = 4f;    // Intervallo tra gli sbatte
    public float closedScaleY = 0.05f;  // La scala Y quando l'occhio è "chiuso"
    
    private Vector3 originalScale;
    private MaskState state = MaskState.Idle;

    void Start()
    {
        // Salviamo la scala originale per sapere come riaprire l'occhio
        if (dxEye != null) 
        {
            originalScale = dxEye.transform.localScale;
        }

        // Avviamo la routine del blink automatico
        if (dxEye != null && sxEye != null)
        {
            StartCoroutine(BlinkScaleRoutine());
        }
    }

    void OnEnable()
    {
        EventMessageManager.OnResetTimer += ResetMask;
        EventMessageManager.OnStartEquipMask += EquipMask;
    }

    void OnDisable()
    {
        EventMessageManager.OnResetTimer -= ResetMask;
        EventMessageManager.OnStartEquipMask -= EquipMask;
    }

    void EquipMask()
    {
        HandleInput();
    }

    // =========================
    // INPUT
    // =========================

    private void HandleInput()
    {
        switch (state)
        {
            case MaskState.Idle:
                StartMask();
                break;

            case MaskState.Running:
                PauseMask();
                break;

            case MaskState.Paused:
                ResumeMask();
                break;

            case MaskState.Finished:
                // Non fa nulla
                break;
        }
    }

    // =========================
    // STATE CHANGES
    // =========================

    private void StartMask()
    {
        state = MaskState.Running;

        mask.SetActive(true);
        SetEyesVisible(true);

        if (maskSlider != null) 
        {
            maskSlider.gameObject.SetActive(false);
        }
    }

    private void PauseMask()
    {
        EventMessageManager.StopUsingMask();
        state = MaskState.Paused;
        SetEyesVisible(false);
    }

    private void ResumeMask()
    {
        state = MaskState.Running;
        SetEyesVisible(true);
    }

    private void FinishMask()
    {
        state = MaskState.Finished;

        EventMessageManager.StopUsingMask();

        mask.SetActive(false);
        SetEyesVisible(true);
    }

    private void ResetMask()
    {
        state = MaskState.Idle;

        mask.SetActive(false);
        SetEyesVisible(true);
    }

    // =========================
    // VISUAL HELPERS & BLINKING
    // =========================

    private void SetEyesVisible(bool visible)
    {
        if (sxEye != null) sxEye.SetActive(visible);
        if (dxEye != null) dxEye.SetActive(visible);
    }

    // --- LOGICA BLINKING AGGIUNTA ---

    private IEnumerator BlinkScaleRoutine()
    {
        while (true)
        {
            // Aspetta un tempo casuale prima di sbattere le palpebre
            yield return new WaitForSeconds(Random.Range(blinkInterval * 0.5f, blinkInterval * 1.5f));

            // Esegui il blink solo se gli occhi sono attivi nella gerarchia (visibili)
            if (dxEye != null && dxEye.activeInHierarchy)
            {
                // --- CHIUSURA ---
                float elapsed = 0;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float newY = Mathf.Lerp(originalScale.y, closedScaleY, elapsed / blinkDuration);
                    SetEyesScale(newY);
                    yield return null;
                }

                // --- APERTURA ---
                elapsed = 0;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float newY = Mathf.Lerp(closedScaleY, originalScale.y, elapsed / blinkDuration);
                    SetEyesScale(newY);
                    yield return null;
                }
                
                // Assicuriamoci che torni esattamente alla scala originale alla fine
                SetEyesScale(originalScale.y);
            }
        }
    }

    private void SetEyesScale(float y)
    {
        if (dxEye == null || sxEye == null) return;

        Vector3 newScale = new Vector3(originalScale.x, y, originalScale.z);
        dxEye.transform.localScale = newScale;
        sxEye.transform.localScale = newScale;
    }
}