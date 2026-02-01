using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    // Evento che verrà lanciato quando metti/togli la maschera
    // true = maschera messa, false = maschera tolta
    public event Action<bool> OnMaskChanged;

    [Header("Feedback Visivo Player")]
    [SerializeField] private Color maskOnColor = new Color(1f, 1f, 1f, 0.5f); // Diventa semi-trasparente
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;

    private bool isMaskActive = false;

    void Awake()
    {
        // Singleton di base
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
        originalColor = spriteRenderer.color;
    }

/*     private void OnEnable()
    {
        // Collega il tasto
        var action = playerInput.actions.FindAction("ShiftMask");
        if (action != null)
            action.performed += ctx => ToggleMask();
    } */

/*     private void OnDisable()
    {
        var action = playerInput.actions.FindAction("ShiftMask");
        if (action != null)
            action.performed -= ctx => ToggleMask();
    }
 */
/*     private void ToggleMask()
    {
        isMaskActive = !isMaskActive;

        // 1. Cambia colore al player per far capire che la maschera è attiva
        spriteRenderer.color = isMaskActive ? maskOnColor : originalColor;

        // 2. Avvisa tutte le piattaforme
        OnMaskChanged?.Invoke(isMaskActive);

        Debug.Log("Stato Maschera: " + isMaskActive);
    } */
    
    // Utile per chi si iscrive in ritardo (es. oggetti spawnati dopo)
    public bool IsMaskActive => isMaskActive;
}