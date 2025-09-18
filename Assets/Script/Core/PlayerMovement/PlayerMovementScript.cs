using System;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public float speed = 5f;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private Vector2 movementInput;

    private bool canMove = true;

    // audioManager // audioManager;

    private void Awake()
    {
        // // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<// audioManager>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.gravityScale = 0f;

        // Optimalisasi Rigidbody
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (!canMove)
        {
            movementInput = Vector2.zero;
            isWalking = false;
            animator.ResetTrigger("isWalking");
            animator.SetTrigger("isIdle");
            return;
        }

        // Ambil input di Update (real-time)
        float horizontal = 0;
        float vertical = 0;

        if (Input.GetKey(KeyCode.W)) vertical = 1;
        if (Input.GetKey(KeyCode.S)) vertical = -1;
        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;

        movementInput = new Vector2(horizontal, vertical).normalized;

        // Cek animasi jalan/idle
        isWalking = (movementInput != Vector2.zero);

        if (isWalking)
        {
            // audioManager.PlaySFX(// audioManager.walkingOnFloor);
            animator.ResetTrigger("isIdle");
            animator.SetTrigger("isWalking");
        }
        else
        {
            animator.ResetTrigger("isWalking");
            animator.SetTrigger("isIdle");
        }

        // Flip sprite
        if (horizontal < 0)
            spriteRenderer.flipX = true;
        else if (horizontal > 0)
            spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isWalking)
        {
            rb.linearVelocity = movementInput * speed;  
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void StopMovement()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        isWalking = false;
        movementInput = Vector2.zero;
        animator.ResetTrigger("isWalking");
        animator.SetTrigger("isIdle");
    }

    public void AllowMovement()
    {
        canMove = true;
    }
}
