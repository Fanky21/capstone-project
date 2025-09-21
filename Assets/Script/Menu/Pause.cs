using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resumeButton;

    [Header("Sub Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject bantuanPanel;
    [SerializeField] private GameObject controlPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf || bantuanPanel.activeSelf || controlPanel.activeSelf)
            {
                BackToPause();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void OnPauseButtonPressed()
    {
        PlayButtonSound();
        if (!isPaused)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        PlayButtonSound();
        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        resumeButton.SetActive(true);

        settingsPanel.SetActive(false);
        bantuanPanel.SetActive(false);
        controlPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        PlayButtonSound();
        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        bantuanPanel.SetActive(false);
        controlPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        PlayButtonSound();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenBantuan()
    {
        PlayButtonSound();
        settingsPanel.SetActive(false);
        controlPanel.SetActive(false);
        pausePanel.SetActive(false);
        bantuanPanel.SetActive(true);
    }

    public void OpenControl()
    {
        PlayButtonSound();
        settingsPanel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public void BackToPause()
    {
        PlayButtonSound();
        settingsPanel.SetActive(false);
        bantuanPanel.SetActive(false);
        controlPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        PlayButtonSound();
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("SavedScene", currentSceneIndex);

        Time.timeScale = 1f;

        if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
    }
}
