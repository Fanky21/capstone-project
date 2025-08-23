using UnityEngine;

public class NPCWander : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float walkTime = 2f;
    public float waitTime = 2f;
    private float walkCounter;
    private float waitCounter;

    private int walkDirection; // 0 = idle, 1 = kanan, 2 = kiri

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection;

    [Tooltip("Cek jika sprite default menghadap kiri.")]
    public bool defaultFacingLeft = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();
    }

    void Update()
    {
        if (walkDirection != 0)
        {
            walkCounter -= Time.deltaTime;

            if (walkCounter <= 0)
            {
                walkDirection = 0;
                waitCounter = waitTime;
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;

            if (waitCounter <= 0)
            {
                ChooseDirection();
            }
        }

        MoveNPC();
    }

    void MoveNPC()
    {
        switch (walkDirection)
        {
            case 0: // Idle
                moveDirection = Vector2.zero;
                break;
            case 1: // Kanan
                moveDirection = Vector2.right;
                break;
            case 2: // Kiri
                moveDirection = Vector2.left;
                break;
        }

        rb.linearVelocity = moveDirection * moveSpeed;

        // Update animasi
        if (animator != null)
        {
            animator.SetBool("isWalking", moveDirection != Vector2.zero);
        }

        // Flip sprite berdasarkan arah dan arah default
        if (moveDirection.x != 0)
        {
            spriteRenderer.flipX = moveDirection.x > 0;
        }

    }

    void ChooseDirection()
    {
        walkDirection = Random.Range(0, 3); // 0=idle, 1=kanan, 2=kiri
        walkCounter = walkTime;
    }
}

// using UnityEngine;

// public class NPCWanderFullDirection : MonoBehaviour
// {
//     public float moveSpeed = 3f;
//     public float walkTime = 2f;
//     public float waitTime = 2f;
//     private float walkCounter;
//     private float waitCounter;

//     private Vector2 moveDirection;

//     private Rigidbody2D rb;
//     private Animator animator;
//     private SpriteRenderer spriteRenderer;

//     [Tooltip("Centang jika sprite default menghadap kiri.")]
//     public bool defaultFacingLeft = true;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         spriteRenderer = GetComponent<SpriteRenderer>();

//         waitCounter = waitTime;
//         walkCounter = walkTime;

//         ChooseDirection();
//     }

//     void Update()
//     {
//         if (moveDirection != Vector2.zero)
//         {
//             walkCounter -= Time.deltaTime;
//             if (walkCounter <= 0)
//             {
//                 moveDirection = Vector2.zero;
//                 waitCounter = waitTime;
//             }
//         }
//         else
//         {
//             waitCounter -= Time.deltaTime;
//             if (waitCounter <= 0)
//             {
//                 ChooseDirection();
//             }
//         }

//         MoveNPC();
//     }

//     void MoveNPC()
//     {
//         rb.linearVelocity = moveDirection * moveSpeed;

//         if (animator != null)
//         {
//             animator.SetBool("isWalking", moveDirection != Vector2.zero);
//         }

//         // Fix arah sprite
//         if (moveDirection.x > 0.01f)
//         {
//             spriteRenderer.flipX = false;
//         }
//         else if (moveDirection.x < -0.01f)
//         {
//             spriteRenderer.flipX = true;
//         }
//     }

//     void ChooseDirection()
//     {

//         int dir = Random.Range(0, 9);

//         switch (dir)
//         {
//             case 0: moveDirection = Vector2.zero; break;           
//             case 1: moveDirection = Vector2.right; break;               
//             case 2: moveDirection = Vector2.left; break;                
//             case 3: moveDirection = Vector2.up; break;                 
//             case 4: moveDirection = Vector2.down; break;                
//             case 5: moveDirection = new Vector2(1, 1).normalized; break;
//             case 6: moveDirection = new Vector2(1, -1).normalized; break;
//             case 7: moveDirection = new Vector2(-1, 1).normalized; break;
//             case 8: moveDirection = new Vector2(-1, -1).normalized; break; 
//         }

//         walkCounter = walkTime;
//     }
// }
