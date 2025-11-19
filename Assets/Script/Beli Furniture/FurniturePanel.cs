using UnityEngine;

public class FurniturePanel : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject panelFurniture;   // Panel besar (UI menu furniture)
    [SerializeField] private GameObject interactButton;   // Tombol "Press F" di UI

    private bool playerInside = false;
    private bool panelOpen = false;

    private void Start()
    {
        if (interactButton != null)
            interactButton.SetActive(false);

        if (panelFurniture != null)
            panelFurniture.SetActive(false);
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.F))
        {
            ToggleFurniturePanel();
        }
    }

    private void ToggleFurniturePanel()
    {
        if (panelOpen)
            CloseFurniturePanel();
        else
            OpenFurniturePanel();
    }

    private void OpenFurniturePanel()
    {
        panelOpen = true;
        panelFurniture.SetActive(true);
        Time.timeScale = 0f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
    }

    private void CloseFurniturePanel()
    {
        panelOpen = false;
        panelFurniture.SetActive(false);
        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;

            if (!panelOpen && interactButton != null)
                interactButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;

            // Hilangkan tombol F
            if (interactButton != null)
                interactButton.SetActive(false);

            // Jika panel sedang terbuka → tutup otomatis
            if (panelOpen)
                CloseFurniturePanel();
        }
    }
}
