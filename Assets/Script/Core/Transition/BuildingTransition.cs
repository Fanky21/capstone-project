using UnityEngine;

public class BuildingTransition : MonoBehaviour
{
    public GameObject indicatorUi;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (gameObject.name == "PcRumah")
            {
        
            }
            else if (gameObject.name == "Kursi")
            {
    
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            indicatorUi.SetActive(true);
            playerInRange = true;
            Debug.Log("Player Detected on :" + gameObject.name);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            indicatorUi.SetActive(false);
            playerInRange = false;
            // Despawn any spawned UI if needed
        }
    }
}
