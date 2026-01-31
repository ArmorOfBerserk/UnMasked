using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    public Transform startPoint;
    public float deathY;

    public Manager manager;
    public Animator animator;

    public bool Key;

    public GameObject YouWinFirstRound;

    [Header("Variabili di movimento")]
    [SerializeField] float movSpeed = 5f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float fallForce = 15f; // Forza manuale premendo "Giù"

    [Header("Hollow Knight Style Gravity")]
    [Tooltip("Moltiplicatore gravità quando scendi (es. 4 per caduta secca)")]
    [SerializeField] private float fallMultiplier = 4f;
    [Tooltip("Velocità massima di caduta")]
    [SerializeField] private float maxFallSpeed = 25f;

    [SerializeField] private Rigidbody2D rb;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    public GameObject PauseMenu;

    private bool isOnGround => groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    private Vector2 moveInput;

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector2.zero;

        playerInput.actions["Jump"].performed += ctx => JumpAction();
        playerInput.actions["EscapeButton"].performed += ctx => PauseGame();
    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled -= ctx => moveInput = Vector2.zero;
        playerInput.actions["Jump"].performed -= ctx => JumpAction();
    }

    private void JumpAction()
    {
        if (isOnGround)
        {
            // Il salto ora è stabile: spinta fissa verso l'alto
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FlipCharacter()
    {
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    private void MoveAction()
    {
        float targetSpeedX = moveInput.x * movSpeed;
        rb.linearVelocity = new Vector2(targetSpeedX, rb.linearVelocity.y);
    }

    private void HandleFastFall()
    {
        if (moveInput.y < -0.5f && !isOnGround)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fallForce);
        }
    }

    // --- LOGICA CADUTA REGOLABILE ---
    private void ApplyCustomGravity()
    {
        // Se la velocità Y è negativa, stiamo cadendo
        if (rb.linearVelocity.y < 0)
        {
            // Applichiamo una gravità extra solo in discesa
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Limita la velocità di caduta per evitare che diventi incontrollabile
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    void Update()
    {
        if (transform.position.y <= deathY)
        {
            manager.Hurt();
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }

        // Gestione Animazioni
        if (!isOnGround) animator.SetInteger("Estado", 2);
        else if (isOnGround && !Mathf.Approximately(moveInput.x, 0f)) animator.SetInteger("Estado", 1);
        else animator.SetInteger("Estado", 0);

        FlipCharacter();
    }

    [Header("Effetti Atterraggio")]
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private float fastFallThreshold = -10f;
    private bool wasInAir;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("key"))
        {
            Key = true;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("gate") && Key)
        {
            YouWinFirstRound.SetActive(true);
            base.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        MoveAction();
        HandleFastFall();
        ApplyCustomGravity(); // <--- Gestisce la velocità di caduta pesante

        if (!isOnGround)
        {
            wasInAir = true;
        }
        else if (wasInAir && isOnGround)
        {
            if (rb.linearVelocity.y < fastFallThreshold)
            {
                SpawnDust();
            }
            wasInAir = false;
        }
    }

    private void SpawnDust()
    {
        if (dustPrefab != null && groundCheck != null)
        {
            Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        PauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
    }
}