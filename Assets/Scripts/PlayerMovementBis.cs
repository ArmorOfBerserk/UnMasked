using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Aggiungiamo questo per essere sicuri che ci sia l'AudioSource
[RequireComponent(typeof(AudioSource))] 
public class PlayerMovementBis : MonoBehaviour
{
    // --- COMPONENTI ---
    private PlayerInput playerInput;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private AudioSource audioSource; // NUOVO

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
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private float fastFallThreshold = -10f;
    public Transform startPoint;

    [Header("Audio")] // SEZIONE NUOVA
    [SerializeField] private AudioClip[] stepSounds; // Array per mettere più suoni di passi diversi
    [SerializeField] private AudioClip deathSound;   // Suono morte
    [SerializeField] private float stepRate = 0.3f;  // Ogni quanto tempo suona il passo (in secondi)
    private float stepTimer;

    [Header("Maschera UI")]
    [SerializeField] Slider maskUI;
    [SerializeField] GameObject hideMask;

    // --- STATO ---
    private Vector2 moveInput;
    private bool wasInAir;
    public bool canMove = true;

    private PlayerAudio playerAudio; // Riferimento al PlayerAudio

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // NUOVO
        playerAudio = GetComponent<PlayerAudio>(); // Otteniamo il riferimento al PlayerAudio

        defaultGravityScale = rb.gravityScale;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
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

    void FixedUpdate()
    {
        if (!canMove) return;

        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);
        CheckLandingDust();
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame && isOnGround)
        {
            EquipMask();
        }

        AnimateCharacter();
        
        if (!canMove) return;

        FlipCharacter();
        HandleFootsteps(); // NUOVO: Gestione suoni passi
    }

    // --- GESTIONE AUDIO PASSI ---
    private void HandleFootsteps()
    {
        // Suoniamo solo se: Siamo a terra E ci stiamo muovendo E non stiamo scivolando contro un muro da fermi
        if (isOnGround && Mathf.Abs(moveInput.x) > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            
            if (stepTimer <= 0)
            {
                playerAudio.PlayAudioPassi(); // Avvia il suono di passo
                stepTimer = stepRate; // Resetta il timer
            }
        }
        else
        {
            // Se mi fermo, resetto il timer così appena riparto il suono è immediato
            stepTimer = 0;
        }
    }

    private void PlayRandomStep()
    {
        if (stepSounds.Length == 0) return;

        // Variazione Pitch (Trucco audio per realismo)
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        
        // Scegli un suono a caso
        int index = Random.Range(0, stepSounds.Length);
        audioSource.PlayOneShot(stepSounds[index]);
    }

    void EquipMask()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        EventMessageManager.StartEquipMask();
    }

    private void CheckLandingDust()
    {
        if (!isOnGround)
        {
            wasInAir = true;
        }
        else if (wasInAir)
        {
            float verticalSpeed = isGravityInverted ? -rb.linearVelocity.y : rb.linearVelocity.y;

            if (verticalSpeed < fastFallThreshold)
            {
                SpawnDust();
                // Opzionale: Aggiungi qui un suono di atterraggio se vuoi
                // PlayRandomStep(); 
            }

            wasInAir = false;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isOnGround)
        {
            playerAudio.PlayAudioSalto(); // Suono salto
            float jumpDirection = isGravityInverted ? -1f : 1f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpDirection);
            anim.SetBool("isJumping", true);
            SpawnDust();
            
            // Opzionale: Aggiungi qui un suono di salto
            // audioSource.pitch = 1f;
            // audioSource.PlayOneShot(jumpSound);
        }
    }

    private void OnToggleGravity(InputAction.CallbackContext ctx)
    {
        isGravityInverted = !isGravityInverted;
        rb.gravityScale = isGravityInverted ? -defaultGravityScale : defaultGravityScale;
        FlipCharacter();
    }

    private void FlipCharacter()
    {
        float scaleY = isGravityInverted ? -1f : 1f;
        float scaleX = transform.localScale.x;

        if (moveInput.x > 0.1f) scaleX = Mathf.Abs(scaleX);
        else if (moveInput.x < -0.1f) scaleX = -Mathf.Abs(scaleX);

        transform.localScale = new Vector3(scaleX, scaleY, 1);
    }

    private void SpawnDust()
    {
        if (dustPrefab != null && groundCheck != null)
        {
            GameObject dust = Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);
            if (isGravityInverted) dust.transform.localScale = new Vector3(1, -1, 1);
        }
    }

    private void OnInteractTriggered(InputAction.CallbackContext ctx)
    {
        if (currentInteractable != null) currentInteractable.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
            currentInteractable = interactable;

        // Morte / Respawn
        if (collision.CompareTag("DeathBox"))
        {
            // 1. Suono Morte
            if (deathSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(deathSound);
            }

            // 2. Reset Camera e Particelle
            if (AutoScrollCamera.Instance != null)
            {
                AutoScrollCamera.Instance.ResetCamera();
                AutoScrollCamera.Instance.ResumeScrolling(); 
            }

            // 3. --- NUOVO: TELETRASPORTA IL TRACKING ---
            if (HeadTrackingReceiver.Instance != null)
            {
                // Questo fa "snappare" la maschera davanti alla camera resettata
                HeadTrackingReceiver.Instance.ForceResetPosition();
            }
            // ------------------------------------------

            // 4. Reset Player
            if (startPoint != null)
            {
                ResetPlayerStatsAndPosition();
            }
        }

        if (collision.CompareTag("StartCameraScroll"))
        {
            EventMessageManager.StartCameraScrolling();
        }
    }

    private void ResetPlayerStatsAndPosition()
    {
        // Questo riabilita il movimento (se vuoi che il player si muova al respawn)
        canMove = true; 
        
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        rb.linearVelocity = Vector2.zero;

        // --- MODIFICA QUI ---
        // Commenta o cancella questa riga se NON vuoi che la maschera si spenga quando muori.
        // EventMessageManager.ResetTimerMask(); 
        // --------------------
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
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