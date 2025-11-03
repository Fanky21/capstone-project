using UnityEngine;

public class BeritaPanel : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject beritaPanel;
    [SerializeField] private GameObject beritaButton;
    [Header("Button Invest")]
    [SerializeField] private GameObject investButton;
    [SerializeField] private GameObject pauseButton;
    private bool isBeritaOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isBeritaOpen)
        {
            ExitPanel();
        }
    }
        
    public void OpenBeritaPanel()
    {
        PlayButtonSound();
        beritaPanel.SetActive(true);
        beritaButton.SetActive(false);
        investButton.SetActive(false);
        pauseButton.SetActive(false);

        Time.timeScale = 0f; 
    }

    public void ExitPanel()
    {
        PlayButtonSound();

        beritaPanel.SetActive(false);
        beritaButton.SetActive(true);
        investButton.SetActive(true);
        pauseButton.SetActive(true);

        Time.timeScale = 1f;
    }

    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
    }
}
