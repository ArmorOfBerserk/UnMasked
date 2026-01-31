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
    }

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector3.zero;
        playerInput.actions["Jump"].performed += ctx => JumpAction();
    }


    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled -= ctx => moveInput = Vector3.zero;
        playerInput.actions["Jump"].performed -= ctx => JumpAction();
    }

    private void JumpAction()
    {
        if (isOnGround)
        {
            Debug.Log("ciaooo");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }
        /*         anim.SetBool("isJumping", !isOnGround);
                anim.SetFloat("yVelocity", rb.linearVelocityY); */


        /* rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); */

    }

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
    }

    void Update()
    {
        FlipCharacter();
        AnimateCharacter();
    }

    void FixedUpdate()
    {
        MoveAction();
        /*         JumpAction(); */
    }
}
