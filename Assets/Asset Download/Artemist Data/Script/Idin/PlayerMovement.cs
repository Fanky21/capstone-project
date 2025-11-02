using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public FloatingJoystick variableJoystick; // joystick UI
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private Vector2 movementInput;
    private bool canMove = true;

    private void Awake()
    {
        // --- LOGIKA SPAWN TERINTEGRASI DIMULAI DI SINI ---
        if (SceneController.instance != null && !string.IsNullOrEmpty(SceneController.instance.TargetSpawnId))
        {
            SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            foreach (SpawnPoint point in spawnPoints)
            {
                if (point.spawnId == SceneController.instance.TargetSpawnId)
                {
                    transform.position = point.transform.position;
                    transform.rotation = point.transform.rotation;

                    Debug.Log($"<color=green>Player position set by PlayerMovement.cs to spawn point '{point.spawnId}'</color>");
                    SceneController.instance.ClearTargetSpawnId();
                    break;
                }
            }
        }
        // --- AKHIR LOGIKA SPAWN ---
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Debug.Log("PlayerMovement initialized at: " + transform.position);
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

        float horizontal = 0;
        float vertical = 0;

        // --- Input dari Keyboard ---
        if (Input.GetKey(KeyCode.W)) vertical = 1;
        if (Input.GetKey(KeyCode.S)) vertical = -1;
        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;

        // --- Input dari Joystick ---
        if (variableJoystick != null)
        {
            // Joystick memiliki prioritas jika digunakan
            if (Mathf.Abs(variableJoystick.Horizontal) > 0.1f || Mathf.Abs(variableJoystick.Vertical) > 0.1f)
            {
                horizontal = variableJoystick.Horizontal;
                vertical = variableJoystick.Vertical;
            }
        }

        movementInput = new Vector2(horizontal, vertical).normalized;
        isWalking = (movementInput != Vector2.zero);

        if (isWalking)
        {
            animator.ResetTrigger("isIdle");
            animator.SetTrigger("isWalking");
        }
        else
        {
            animator.ResetTrigger("isWalking");
            animator.SetTrigger("isIdle");
        }

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
