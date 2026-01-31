using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private IInteractable currentInteractable;

    [Header("Variabili di movimento")]
    [SerializeField] float movSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] private Rigidbody2D rb;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    //Da settare a mano per ogni livello
    public Transform startPoint;

    private Vector2 moveInput;
    private bool wasInAir;
    private bool isOnGround => groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    [Header("Effetti Atterraggio")]
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private float fastFallThreshold = -10f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
    }

    void OnEnable()
    {
        moveAction.performed += OnMove;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJump;
        if (interactAction != null) interactAction.performed += OnInteractTriggered;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJump;
        if (interactAction != null) interactAction.performed -= OnInteractTriggered;
    }

    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isOnGround)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }
    }

    private void OnInteractTriggered(InputAction.CallbackContext ctx)
    {
        TryInteract();
    }

    private void TryInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
            currentInteractable = interactable;

        if (collision.tag == "DeathBox")
        {
            /*  manager.Hurt(); */
            Debug.Log("CIAO");
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable) currentInteractable = null;
        }
    }

    void Update()
    {
        FlipCharacter();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);

        // Logica polvere/atterraggio
        if (!isOnGround) wasInAir = true;
        else if (wasInAir)
        {
            if (rb.linearVelocity.y < fastFallThreshold) SpawnDust();
            wasInAir = false;
            anim.SetBool("isJumping", false);
        }


    }

    private void FlipCharacter()
    {
        if (Mathf.Abs(moveInput.x) > 0.1f)
            spriteRenderer.flipX = moveInput.x < 0;
    }

    private void SpawnDust()
    {
        if (dustPrefab != null && groundCheck != null)
            Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);
    }


}