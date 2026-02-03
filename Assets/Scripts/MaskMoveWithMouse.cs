using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MaskMoveWithMouse : MonoBehaviour
{
    [Header("Impostazioni Modalità")]
    public bool useHeadTracking = false;
    public bool canBlink = true;

    [Header("Movimento")]
    public float smooth = 15f;
    public float mouseOffsetX = 100f;

    [Header("Head Tracking")]
    public float headTrackingIntensity = 0.15f;
    public float headTrackingSpeed = 1.5f;
    public float activationInterval = 2f;

    [Header("Blinking (Scaling)")]
    public Transform eyeDx;
    public Transform eyeSx;
    public float blinkDuration = 0.1f; 
    public float blinkInterval = 4f;  
    public float closedScaleY = 0.05f;  

    private float noiseTimer;
    private Vector3 originalScale;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

    
        if (eyeDx != null)
        {
         
            if (eyeDx.localScale.y > closedScaleY + 0.01f)
            {
                originalScale = eyeDx.localScale;
            }
        }

        if (canBlink && eyeDx != null && eyeSx != null)
        {
            StartCoroutine(BlinkScaleRoutine());
        }
    }

    void OnDisable()
    {
        if (eyeDx != null && originalScale != Vector3.zero)
        {
            SetEyesScale(originalScale.y);
        }
    }

    void Update()
    {
        HandleMovement();
    }



    private void HandleMovement()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x + mouseOffsetX, mouseScreenPos.y, -Camera.main.transform.position.z)
        );

        Vector3 targetPos = mouseWorldPos;

        if (useHeadTracking)
        {
            noiseTimer += Time.deltaTime * headTrackingSpeed;
            float noiseX = Mathf.PerlinNoise(noiseTimer, 0) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0, noiseTimer) - 0.5f;

            float wave = Mathf.Sin(Time.time * (2f * Mathf.PI / activationInterval));
            float currentIntensity = (wave > 0.7f) ? headTrackingIntensity * 2f : headTrackingIntensity;

            targetPos += new Vector3(noiseX, noiseY, 0) * currentIntensity;
        }

        targetPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smooth);
    }

 
    private IEnumerator BlinkScaleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkInterval * 0.5f, blinkInterval * 1.5f));

            if (canBlink)
            {
               
                float elapsed = 0;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
              
                    float t = Mathf.Clamp01(elapsed / blinkDuration);
                    float newY = Mathf.Lerp(originalScale.y, closedScaleY, t);
                    SetEyesScale(newY);
                    yield return null;
                }
          
                SetEyesScale(closedScaleY);

                // --- APERTURA ---
                elapsed = 0;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / blinkDuration);
                    float newY = Mathf.Lerp(closedScaleY, originalScale.y, t);
                    SetEyesScale(newY);
                    yield return null;
                }
         
                SetEyesScale(originalScale.y);
            }
        }
    }

    private void SetEyesScale(float y)
    {
        Vector3 newScale = new Vector3(originalScale.x, y, originalScale.z);
        eyeDx.localScale = newScale;
        eyeSx.localScale = newScale;

    }
}