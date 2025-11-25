using UnityEngine;

public class InvestPanel : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject investPanel;
    [SerializeField] private GameObject investButton;

    [Header("Sub Panels")]
    [SerializeField] private GameObject cryptoPanel;
    [SerializeField] private GameObject sahamPanel;
    [Header("Button Berita")]
    [SerializeField] private GameObject pauseButton;

    private bool isInvestOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isInvestOpen)
        {
            ExitPanel();
        }
    }

    public void OpenInvestPanel()
    {
        PlayButtonSound();
        isInvestOpen = true;

        investPanel.SetActive(true);
        investButton.SetActive(false);
        pauseButton.SetActive(false);

        cryptoPanel.SetActive(true);
        sahamPanel.SetActive(false);

        Time.timeScale = 0f; // pause game
    }

    public void OpenCryptoPanel()
    {
        PlayButtonSound();
        cryptoPanel.SetActive(true);
        sahamPanel.SetActive(false);
    }

    public void OpenSahamPanel()
    {
        PlayButtonSound();
        sahamPanel.SetActive(true);
        cryptoPanel.SetActive(false);
    }

    public void ExitPanel()
    {
        PlayButtonSound();
        isInvestOpen = false;

        investPanel.SetActive(false);
        investButton.SetActive(true);
        pauseButton.SetActive(true);

        cryptoPanel.SetActive(false);
        sahamPanel.SetActive(false);

        Time.timeScale = 1f; // resume game
    }

    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
    }
}
