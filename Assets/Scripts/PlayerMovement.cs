using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    
    // Riferimento all'oggetto interagibile
    private IInteractable currentInteractable;

    [Header("Variabili di movimento")]
    [SerializeField] float movSpeed = 8f;
    [SerializeField] float jumpForce = 12f;

    [SerializeField] private Rigidbody2D rb;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // STATO GRAVITA'
    private float defaultGravityScale;
    private bool isGravityInverted = false;

    private bool isOnGround => groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    
    private Vector2 moveInput;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        // Salviamo la gravità originale all'avvio (es. 1 o 3)
        defaultGravityScale = rb.gravityScale;
    }

    private void OnEnable()
    {
        // 1. MOVIMENTO
        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector3.zero;
        
        // 2. SALTO
        playerInput.actions["Jump"].performed += ctx => JumpAction();

        // 3. INTERAZIONE (NPC/Cartelli)
        var interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null) interactAction.performed += ctx => TryInteract();

        // 4. NUOVA AZIONE: GRAVITA'
        // Assicurati che nell'Input System si chiami esattamente "gravityMask"
        var gravityAction = playerInput.actions.FindAction("gravityMask");
        if (gravityAction != null)
        {
            gravityAction.performed += ctx => ToggleGravity();
        }
        else
        {
            Debug.LogError("Non trovo l'azione 'gravityMask'! Controlla il file .inputactions");
        }
    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled -= ctx => moveInput = Vector3.zero;
        playerInput.actions["Jump"].performed -= ctx => JumpAction();

        var interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null) interactAction.performed -= ctx => TryInteract();

        var gravityAction = playerInput.actions.FindAction("gravityMask");
        if (gravityAction != null) gravityAction.performed -= ctx => ToggleGravity();
    }

    // --- LOGICA GRAVITA' ---
    private void ToggleGravity()
    {
        isGravityInverted = !isGravityInverted;

        // 1. Inverti la fisica
        // Se invertita diventa negativa, altrimenti torna al default positivo
        rb.gravityScale = isGravityInverted ? -defaultGravityScale : defaultGravityScale;

        // 2. Capovolgi visivamente il personaggio (Flip Y)
        // Manteniamo la scala X attuale (che gestisce la direzione destra/sinistra)
        float currentScaleX = transform.localScale.x;
        
        // Impostiamo Y a -1 (testa in giù) o 1 (normale)
        // Questo sposta automaticamente anche il GroundCheck che è figlio del player!
        transform.localScale = new Vector3(currentScaleX, isGravityInverted ? -1 : 1, 1);

        // Opzionale: Azzera la velocità verticale per evitare inerzie strane durante lo switch
        // rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }

    private void JumpAction()
    {
        if (isOnGround)
        {
            // Se la gravità è invertita (siamo sul soffitto), dobbiamo spingere verso il basso (negativo)
            // Se è normale, spingiamo verso l'alto (positivo)
            float jumpDirection = isGravityInverted ? -1f : 1f;
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpDirection);
        }
    }

    private void FlipCharacter()
    {
        // Gestione direzione Destra/Sinistra
        // Usiamo Mathf.Abs per essere sicuri di prendere il valore positivo della scala
        // e poi lo moltiplichiamo per 1 o -1 a seconda della direzione
        
        if (moveInput.x > 0.1f)
        {
            // Guarda a destra: X positiva
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x); 
            transform.localScale = s;
        }
        else if (moveInput.x < -0.1f)
        {
            // Guarda a sinistra: X negativa
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
        
        // NOTA: Ho cambiato il sistema di Flip rispetto a prima. 
        // Invece di usare spriteRenderer.flipX, uso la Scala X negativa.
        // Questo è necessario perché stiamo già manipolando la Scala Y per la gravità
        // e mescolare spriteRenderer.flipX con transform.scale.y negativo può creare bug visivi.
    }

    // ... Interazioni e Aggiornamenti ...
    private void TryInteract()
    {
        if (currentInteractable != null) currentInteractable.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable)) currentInteractable = interactable;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable) currentInteractable = null;
            if (InteractionUI.Instance != null && InteractionUI.Instance.IsOpen) InteractionUI.Instance.CloseWindow();
        }
    }

    private void MoveAction()
    {
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);
    }

    void Update()
    {
        FlipCharacter();
        // AnimateCharacter();
    }

    void FixedUpdate()
    {
        MoveAction();
    }
}