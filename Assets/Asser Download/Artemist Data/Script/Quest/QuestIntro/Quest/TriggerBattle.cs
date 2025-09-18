using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerBattle : MonoBehaviour
{
    [SerializeField] private string sceneName; // Nama scene tujuan, isi di Inspector

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            SceneManager.LoadScene(sceneName);
        }
    }
}