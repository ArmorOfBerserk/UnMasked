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
    }

    private void OnEnable()
    {
        // Movimento
        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector3.zero;
        
        // Salto
        playerInput.actions["Jump"].performed += ctx => JumpAction();

        // INTERAZIONE IMMEDIATA
        // "performed" scatta al frame esatto della pressione
        var interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += ctx => TryInteract();
        }
    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled -= ctx => moveInput = Vector3.zero;
        playerInput.actions["Jump"].performed -= ctx => JumpAction();

        var interactAction = playerInput.actions.FindAction("Interact");
        if (interactAction != null)
            interactAction.performed -= ctx => TryInteract();
    }

    private void TryInteract()
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

    private void JumpAction()
    {
        if (isOnGround)
        {
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

    private void AnimateCharacter()
    {
        // Logica animazione qui
    }

    private void MoveAction()
    {
        rb.linearVelocity = new Vector2(moveInput.x * movSpeed, rb.linearVelocity.y);
    }

    void Update()
    {
        FlipCharacter();
        AnimateCharacter();
    }

    void FixedUpdate()
    {
        MoveAction();
    }
}