using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // --- COMPONENTI ---
    private PlayerInput playerInput;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // --- INTERAZIONE ---
    private IInteractable currentInteractable;

    // --- INPUT ACTIONS ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction gravityAction;

    [Header("Variabili di movimento")]
    [SerializeField] float movSpeed = 8f;
    [SerializeField] float jumpForce = 12f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isOnGround => groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    [Header("Sistema Gravità")]
    private float defaultGravityScale;
    private bool isGravityInverted = false;

    [Header("Effetti & Respawn")]
    [SerializeField] private GameObject dustPrefab;     // Particelle atterraggio
    [SerializeField] private float fastFallThreshold = -10f; // Soglia velocità per polvere
    public Transform startPoint;

    [Header("Maschera UI")]
    [SerializeField] Slider maskUI;
    [SerializeField] GameObject hideMask;

    // --- STATO ---
    private Vector2 moveInput;
    private bool wasInAir;
    public bool canMove = true;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Salviamo la gravità originale all'avvio
        defaultGravityScale = rb.gravityScale;

        // Cache delle azioni per pulizia
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];

        // Assicurati che l'azione esista nel tuo Input System
        gravityAction = playerInput.actions.FindAction("gravityMask");
        ResetPlayerStatsAndPosition();
    }



    void OnEnable()
    {
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;
        EventMessageManager.OnLockPlayerMovement += () => canMove = true;

        jumpAction.performed += OnJump;

        if (interactAction != null) interactAction.performed += OnInteractTriggered;
        if (gravityAction != null) gravityAction.performed += OnToggleGravity;
    }

    void OnDisable()
    {
        moveAction.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled -= ctx => moveInput = Vector2.zero;

        jumpAction.performed -= OnJump;

        if (interactAction != null) interactAction.performed -= OnInteractTriggered;
        if (gravityAction != null) gravityAction.performed -= OnToggleGravity;
    }

    // --- LOGICA MOVIMENTO & FISICA ---

    void FixedUpdate()
    {
        if (!canMove)
            return;

        // Movimento orizzontale
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);

        // Logica Polvere (Dust) all'atterraggio
        CheckLandingDust();
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame && isOnGround)
        {
            EquipMask();
        }

        AnimateCharacter();

        if (!canMove)
            return;

        FlipCharacter();
    }

    void EquipMask()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        EventMessageManager.StartEquipMask();
    }

    private void CheckLandingDust()
    {
        // Se non siamo a terra, segniamo che siamo in aria
        if (!isOnGround)
        {
            wasInAir = true;
        }
        // Se eravamo in aria e ora tocchiamo terra -> Atterraggio!
        else if (wasInAir)
        {
            // Spawn particelle solo se cadevamo veloce (o se la gravità è invertita, se salivamo veloce)
            float verticalSpeed = isGravityInverted ? -rb.linearVelocity.y : rb.linearVelocity.y;

            if (verticalSpeed < fastFallThreshold)
            {
                SpawnDust();
            }

            wasInAir = false;
            anim.SetBool("isJumping", false);
        }
    }

    // --- LOGICA SALTO ---

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isOnGround)
        {
            // Se gravità invertita, saltiamo verso il basso (-1), altrimenti verso l'alto (1)
            float jumpDirection = isGravityInverted ? -1f : 1f;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpDirection);

            anim.SetBool("isJumping", true);

            // Opzionale: Dust anche quando salti
            SpawnDust();
        }
    }

    // --- LOGICA GRAVITA' ---

    private void OnToggleGravity(InputAction.CallbackContext ctx)
    {
        isGravityInverted = !isGravityInverted;

        // 1. Inverti la fisica
        rb.gravityScale = isGravityInverted ? -defaultGravityScale : defaultGravityScale;

        // 2. Capovolgi visivamente il personaggio (Flip Y)
        // Usiamo Scale invece di FlipY dello sprite per girare anche il GroundCheck!
        FlipCharacter();
    }

    private void ResetGravity()
    {
        // Utile quando si muore per resettare lo stato
        isGravityInverted = false;
        rb.gravityScale = defaultGravityScale;
        FlipCharacter(); // Riapplica la scala corretta
    }

    // --- LOGICA VISIVA (FLIP) ---

    private void FlipCharacter()
    {
        // Gestione Scala Y (Gravità)
        float scaleY = isGravityInverted ? -1f : 1f;

        // Gestione Scala X (Direzione)
        // Manteniamo la X corrente ma ne forziamo il segno
        float scaleX = transform.localScale.x;

        if (moveInput.x > 0.1f)
        {
            scaleX = Mathf.Abs(scaleX); // Guarda a destra (Positivo)
        }
        else if (moveInput.x < -0.1f)
        {
            scaleX = -Mathf.Abs(scaleX); // Guarda a sinistra (Negativo)
        }

        // Applichiamo la trasformazione completa
        transform.localScale = new Vector3(scaleX, scaleY, 1);
    }

    private void SpawnDust()
    {
        if (dustPrefab != null && groundCheck != null)
        {
            // Creiamo la polvere
            GameObject dust = Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);

            // Se siamo a testa in giù, giriamo anche la polvere!
            if (isGravityInverted)
            {
                dust.transform.localScale = new Vector3(1, -1, 1);
            }
        }
    }

    // --- INTERAZIONE & RESPAWN ---

    private void OnInteractTriggered(InputAction.CallbackContext ctx)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Interazione
        if (collision.TryGetComponent(out IInteractable interactable))
            currentInteractable = interactable;

        // Morte / Respawn
        if (collision.CompareTag("DeathBox"))
        {
            // Resetta la posizione
            if (startPoint != null)
            {
                ResetPlayerStatsAndPosition();
            }
        }
    }

    private void ResetPlayerStatsAndPosition()
    {
        canMove = true;
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        EventMessageManager.ResetTimerMask();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                // Chiude l'UI se ti allontani
                if (InteractionUI.Instance != null && InteractionUI.Instance.IsOpen)
                    InteractionUI.Instance.CloseWindow();
            }
        }
    }

    private void AnimateCharacter()
    {
        anim.SetFloat("run", Mathf.Abs(rb.linearVelocityX), 0.01f, Time.fixedDeltaTime);
        anim.SetBool("isJumping", !isOnGround);
        anim.SetFloat("yVelocity", rb.linearVelocityY);
        anim.SetBool("isEquipMask", !canMove);
    }
}