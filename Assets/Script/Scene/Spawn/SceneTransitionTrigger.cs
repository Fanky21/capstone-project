using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("The name of the scene to load.")]
    [SerializeField] private string targetSceneName;

    [Tooltip("The ID of the SpawnPoint in the destination scene where the player should appear.")]
    [SerializeField] private string targetSpawnPointId;

    private float stayTime = 0f;
    private bool playerInside = false;
    private Rigidbody2D playerRigidbody;
    private MapMovementAnimation mapMovementAnimation;

    private void Start()
    {
        // Find MapMovementAnimation once at start for efficiency
        mapMovementAnimation = FindFirstObjectByType<MapMovementAnimation>();
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
        if (playerInside && playerRigidbody != null)
        {
            if (playerRigidbody.linearVelocity.magnitude < 0.01f)
            {
                stayTime += Time.deltaTime;

                if (stayTime >= 0.3f)
                {
                    if (mapMovementAnimation != null)
                    {
                        mapMovementAnimation.startTransitionAnimation();
                    }
                    else
                    {
                        Debug.LogWarning("MapMovementAnimation not found in scene!");
                    }

                    if (stayTime >= 0.3f)
                    {
                        MoveScene();
                        playerInside = false; // Prevent re-triggering
                    }
                }
            }
            else
            {
                stayTime = 0f;
            }
        }
    }

    private void MoveScene()
    {
        // Try to use SceneController if available
        if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene(targetSceneName, targetSpawnPointId);
        }
        else
        {
            // Fallback to SceneTransitionManager if available
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.SetSceneAsal(SceneManager.GetActiveScene().name);
            }
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
