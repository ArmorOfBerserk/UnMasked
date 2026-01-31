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
    private float lastPosition;

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
    private Coroutine currentShootCoroutine;

    private Vector2 moveInput;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
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
        if (isOnGround)
        {
            Debug.Log("ciaooo");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }
        /*         anim.SetBool("isJumping", !isOnGround);
                anim.SetFloat("yVelocity", rb.linearVelocityY); */


        /* rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); */

    //}

    private void FlipCharacter()
    {
        if (moveInput.x > 0.1f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.1f)
        {
            spriteRenderer.flipX = true;

        }

    }

    private void AnimateCharacter()
    {
        /*         anim.SetFloat("run", Mathf.Abs(rb.linearVelocityX), 0.01f, Time.fixedDeltaTime);
                anim.SetBool("isJumping", !isOnGround);
                anim.SetFloat("yVelocity", rb.linearVelocityY); */
    }

    private void MoveAction()
    {
        /* transform.position += new Vector3(moveInput.x * movSpeed * Time.deltaTime, 0, 0); */
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
