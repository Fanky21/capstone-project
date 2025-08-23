using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class MainMenuTransitionScript : MonoBehaviour
{
    public GameObject mainMenuMain;
    public GameObject mainMenuSettings;
    public GameObject mainMenuTransition;
    public Image mainmenuTransitionImage;
    public GameObject mainMenuIntroTransition;
    public Image mainMenuImageIntroTransition;

    // Buttons
    public Button settingsButton;
    public Button videoSettingsBackButton;

    void Start()
    {
        StartCoroutine(ActivateDimmerAfterDelay());
    }

    private IEnumerator ActivateDimmerAfterDelay()
    {
        mainMenuIntroTransition.SetActive(true);

        // Animate the opacity of MainMenuDimmer with ease-out effect
        if (mainMenuImageIntroTransition != null)
        {
            Color dimmerColor = mainMenuImageIntroTransition.color;
            float duration = 1f; // Animation duration in seconds
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease-out cubic interpolation
                dimmerColor.a = Mathf.Lerp(1f, 0f, t);
                mainMenuImageIntroTransition.color = dimmerColor;
                yield return null;
            }

            dimmerColor.a = 0f;
            mainMenuImageIntroTransition.color = dimmerColor;
            
            mainMenuIntroTransition.SetActive(false);
        }
    }

    public void startSettingsTransition()
    {
        settingsButton.interactable = false;

        Debug.Log("Start Settings Transition");

        mainMenuTransition.SetActive(true);

        RectTransform rect = mainmenuTransitionImage.GetComponent<RectTransform>();
        Vector2 targetSize = new Vector2(4000f, 4000f);

        LeanTween.value(mainmenuTransitionImage.gameObject, rect.sizeDelta, targetSize, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((Vector2 val) =>
            {
                rect.sizeDelta = val;
            })
            .setOnComplete(() =>
            {
                mainMenuMain.SetActive(false);
                mainMenuSettings.SetActive(true);
                settingsButton.interactable = true;
                returnToOriginalSize();
            });
    }

    public void exitSettingsTransition()
    {
        videoSettingsBackButton.interactable = false;

        Debug.Log("Exit Settings Transition");

        mainMenuTransition.SetActive(true);

        RectTransform rect = mainmenuTransitionImage.GetComponent<RectTransform>();
        Vector2 targetSize = new Vector2(4000f, 4000f);

        LeanTween.value(mainmenuTransitionImage.gameObject, rect.sizeDelta, targetSize, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((Vector2 val) =>
            {
                rect.sizeDelta = val;
            })
            .setOnComplete(() =>
            {
                mainMenuMain.SetActive(true);
                mainMenuSettings.SetActive(false);
                videoSettingsBackButton.interactable = true;
                returnToOriginalSize();
            });
    }

    void returnToOriginalSize()
    {



        RectTransform rect = mainmenuTransitionImage.GetComponent<RectTransform>();
        Vector2 targetSize = new Vector2(0f, 0f);

        LeanTween.value(mainmenuTransitionImage.gameObject, rect.sizeDelta, targetSize, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((Vector2 val) =>
            {
                rect.sizeDelta = val;
            })
            .setOnComplete(() =>
            {
                mainMenuTransition.SetActive(false);
            });

    }

}
