using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public FloatingJoystick variableJoystick;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isWalking = false;
    private Vector2 movementInput;
    private bool canMove = true;

    public float walkSfxTimer;
    public float walkSfxInterval;

    private void Awake()
    {
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
    }

    void Update()
    {
        if (!canMove)
        {
            StopAnimation();
            return;
        }

        float horizontal = 0;
        float vertical = 0;

        if (Input.GetKey(KeyCode.W)) vertical = 1;
        if (Input.GetKey(KeyCode.S)) vertical = -1;
        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;

        if (variableJoystick != null)
        {
            if (Mathf.Abs(variableJoystick.Horizontal) > 0.1f ||
                Mathf.Abs(variableJoystick.Vertical) > 0.1f)
            {
                horizontal = variableJoystick.Horizontal;
                vertical = variableJoystick.Vertical;
            }
        }

        movementInput = new Vector2(horizontal, vertical).normalized;
        isWalking = movementInput != Vector2.zero;

        HandleAnimation();
        HandleSound();
        HandleFlip(horizontal);
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = isWalking ? movementInput * speed : Vector2.zero;
    }

    void HandleAnimation()
    {
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
    }

    void HandleSound()
    {
        if (isWalking)
        {
            walkSfxTimer += Time.deltaTime;
            if (walkSfxTimer >= walkSfxInterval)
            {
                walkSfxTimer = 0f;
                SoundManager.Instance.PlaySound2D("Walk");
            }
        }
        else
        {
            walkSfxTimer = 0f;
        }
    }

    void HandleFlip(float horizontal)
    {
        if (horizontal < 0)
            spriteRenderer.flipX = true;
        else if (horizontal > 0)
            spriteRenderer.flipX = false;
    }

    void StopAnimation()
    {
        movementInput = Vector2.zero;
        isWalking = false;
        animator.ResetTrigger("isWalking");
        animator.SetTrigger("isIdle");
    }

    public void StopMovement()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        StopAnimation();
    }

    public void AllowMovement()
    {
        canMove = true;
    }
}
