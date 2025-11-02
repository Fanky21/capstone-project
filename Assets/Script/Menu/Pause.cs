using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject bantuanPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (bantuanPanel.activeSelf)
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
        bantuanPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        PlayButtonSound();
        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        bantuanPanel.SetActive(false);
    }

    public void OpenBantuan()
    {
        PlayButtonSound();
        pausePanel.SetActive(false);
        bantuanPanel.SetActive(true);
    }

    public void BackToPause()
    {
        PlayButtonSound();
        bantuanPanel.SetActive(false);
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
