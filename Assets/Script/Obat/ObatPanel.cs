using UnityEngine;

public class ObatPanel : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject panelObat;   
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

        if (panelObat != null)
            panelObat.SetActive(false);
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
        panelObat.SetActive(true);
        investButton.SetActive(false);
        pauseButton.SetActive(false);
        Time.timeScale = 0f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");
    }

    public void CloseFurniturePanel()
    {
        panelOpen = false;
        panelObat.SetActive(false);
        investButton.SetActive(true);
        pauseButton.SetActive(true);
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

            if (interactButton != null)
                interactButton.SetActive(false);

            if (panelOpen)
                CloseFurniturePanel();
        }
    }
}
