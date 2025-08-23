using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public GameObject interactionUI;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactionUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (gameObject.name == "PcRumah")
                {
                    // Spawn the specific UI for this object
                } else if (gameObject.name == "Kursi")
                {
                    // Spawn the specific UI for this object
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactionUI.SetActive(false);
        }
        
        // despawn jika ada yang di spawn diatas
    }
}