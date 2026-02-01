using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum MaskState
{
    Idle,
    Running,
    Paused,
    Finished
}


public class PlayerMask : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float timeout = 10f;

    [Header("References")]
    [SerializeField] private GameObject mask;
    [SerializeField] private Slider maskSlider;
    [SerializeField] private GameObject dxEye;
    [SerializeField] private GameObject sxEye;

    private Coroutine maskCoroutine;
    private float currentTime;
    private MaskState state = MaskState.Idle;



    void EquipMask()
    {
        HandleInput();
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
        currentTime = timeout;

        mask.SetActive(true);
        SetEyesVisible(true);

        if (maskSlider != null)
        {
            maskSlider.maxValue = timeout;
            maskSlider.value = timeout;
        }

        maskCoroutine = StartCoroutine(MaskTimer());
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

        if (maskCoroutine != null)
        {
            StopCoroutine(maskCoroutine);
            maskCoroutine = null;
        }
    }

    private void ResetMask()
    {

        state = MaskState.Idle;
        currentTime = timeout;

        if (maskCoroutine != null)
        {
            StopCoroutine(maskCoroutine);
            maskCoroutine = null;
        }

        mask.SetActive(false);
        SetEyesVisible(true);

        if (maskSlider != null)
        {
            maskSlider.value = timeout;
        }
    }

    // =========================
    // TIMER
    // =========================

    private IEnumerator MaskTimer()
    {
        while (currentTime > 0f)
        {
            if (state == MaskState.Running)
            {
                currentTime -= Time.deltaTime;

                if (maskSlider != null)
                {
                    maskSlider.value = currentTime;
                }
            }

            yield return null;
        }

        FinishMask();
    }

    // =========================
    // VISUAL HELPERS
    // =========================

    private void SetEyesVisible(bool visible)
    {
        sxEye.SetActive(visible);
        dxEye.SetActive(visible);
    }
}
