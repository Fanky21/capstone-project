using UnityEngine;

public class PlayerMovement_Indoor : MonoBehaviour
{
    public float speed = 3f;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private Vector2 movementInput;

    AudioManager audioManager;

    // private void Awake()
    // {
    //     audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    // }

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
    private bool canMove = true;

    [System.Obsolete]
    public void StopMovement()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        isWalking = false;
        movementInput = Vector2.zero;
        animator.ResetTrigger("isWalking");
        animator.SetTrigger("isIdle");
    }

    public void AllowMovement()
    {
        canMove = true;
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

        // Ambil input horizontal saja
        float horizontal = 0;

        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;

        movementInput = new Vector2(horizontal, 0).normalized;

        // Cek animasi jalan/idle
        isWalking = (movementInput != Vector2.zero);

        if (isWalking)
        {
            // audioManager.PlaySFX(audioManager.walkingOnFloor);
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
}