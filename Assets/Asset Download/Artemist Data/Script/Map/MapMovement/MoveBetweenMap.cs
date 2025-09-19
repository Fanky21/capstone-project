// using UnityEditor.SearchService;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MoveBetweenMap : MonoBehaviour
// {

//     // Target Map
//     public string sceneTargetName;

//     // Move Between Scene
//     void moveScene()
//     {
//         SceneManager.LoadScene(sceneTargetName);
//     }

//     private float stayTime = 0f;
//     private bool playerInside = false;
//     private Vector3 lastPlayerPosition;
//     private Rigidbody2D playerRigidbody;

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             Debug.Log("Player entered the trigger zone.");

//             playerInside = true;
//             lastPlayerPosition = other.transform.position;
//             playerRigidbody = other.GetComponent<Rigidbody2D>();
//             stayTime = 0f;
//         }
//     }

//     private void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             playerInside = false;
//             stayTime = 0f;
//             playerRigidbody = null;
//         }
//     }

//     [System.Obsolete]
//     private void Update()
//     {
//         if (playerInside && playerRigidbody != null)
//         {
//             // Check if player is not moving (velocity close to zero)
//             if (playerRigidbody.velocity.magnitude < 0.01f)
//             {
//                 stayTime += Time.deltaTime;
//                 if (stayTime >= 1f)
//                 {
//                     moveScene();
//                     playerInside = false; // Prevent multiple triggers
//                 }
//             }
//             else
//             {
//                 stayTime = 0f;
//             }
//         }
//     }
// }

using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveBetweenMap : MonoBehaviour
{
    // Target Map
    public string sceneTargetName;

    private float stayTime = 0f;
    private bool playerInside = false;
    private Rigidbody2D playerRigidbody;
    private MapMovementAnimation mapMovementAnimation;

    void MoveScene()
    {
        // Simpan nama scene asal
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.SetSceneAsal(SceneManager.GetActiveScene().name);
        }

        // Pindah scene
        SceneManager.LoadScene(sceneTargetName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger zone.");
            playerInside = true;
            playerRigidbody = other.GetComponent<Rigidbody2D>();
            stayTime = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            stayTime = 0f;
            playerRigidbody = null;
        }
    }

    private void Update()
    {
        MapMovementAnimation mapMovementAnimation = FindFirstObjectByType<MapMovementAnimation>();

        if (playerInside && playerRigidbody != null)
        {
            if (playerRigidbody.linearVelocity.magnitude < 0.01f)
            {
                stayTime += Time.deltaTime;
                if (stayTime >= 2f)
                {
                    if (mapMovementAnimation != null)
                    {
                        mapMovementAnimation.startTransitionAnimation();
                    }
                    else
                    {
                        Debug.LogWarning("MapMovementAnimation not found in scene!");
                    }

                    if (stayTime >= 2.5f)
                    {
                        MoveScene();
                        playerInside = false;
                    }
                }
            }
            else
            {
                stayTime = 0f;
            }
        }
    }
}
