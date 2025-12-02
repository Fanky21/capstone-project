using UnityEngine;

public class FurniturePanel : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject panelFurniture;   
    [SerializeField] private GameObject interactButton;  
    [Header("Button Invest")]
    [SerializeField] private GameObject investButton;
    [SerializeField] private GameObject pauseButton;

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

    public void OpenFurniturePanel()
    {
        panelOpen = true;
        panelFurniture.SetActive(true);
        investButton.SetActive(false);
        pauseButton.SetActive(false);
        Time.timeScale = 0f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Irishaimase");
    }

    public void CloseFurniturePanel()
    {
        panelOpen = false;
        panelFurniture.SetActive(false);
        investButton.SetActive(true);
        pauseButton.SetActive(true);
        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Arigato");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;

            if (!panelOpen && interactButton != null)
                interactButton.SetActive(true);
                
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("Irishaimase");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;

            if (interactButton != null)
                interactButton.SetActive(false);

            if (panelOpen)
                CloseFurniturePanel();
        }
    }
}
