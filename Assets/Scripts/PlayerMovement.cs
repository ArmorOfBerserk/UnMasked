using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
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
    [SerializeField] float movSpeed;
    [SerializeField] float jumpForce;
    private InputAction moveAction;
    private InputAction jumpAction;



    [SerializeField] private Rigidbody2D rb;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;

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
        
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

    }

    void OnEnable()
    {
        moveAction.performed += OnMove;
        moveAction.canceled  += OnMoveCanceled;
        
        jumpAction.performed += OnJump;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled  -= OnMoveCanceled;
        
        jumpAction.performed -= OnJump;
    }
    
    
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isOnGround) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        anim.SetBool("isJumping", true);
    }

        // INTERAZIONE IMMEDIATA
        // "performed" scatta al frame esatto della pressione
        var interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += ctx => TryInteract();
        }
    }

    
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }



    /*private void JumpAction()
    {
        // Se c'è un oggetto nel raggio, interagisce subito
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    // Rilevamento collisioni per interazione
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                // Chiude la finestra se ti allontani
                if (InteractionUI.Instance != null && InteractionUI.Instance.IsOpen)
                    InteractionUI.Instance.CloseWindow();
            }
        }
    }

    //}

    private void FlipCharacter()
    {
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    private void AnimateCharacter()
    {
        // Logica animazione qui
    }

    private void MoveAction()
    {
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);
        anim.SetBool("move", true);
    }

    void Update()
    {
        FlipCharacter();
        AnimateCharacter();
    }

    void FixedUpdate()
    {    
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);

        anim.SetBool("move", Mathf.Abs(moveInput.x) > 0.01f);

    }
}