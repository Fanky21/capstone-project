using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class MangaStyleDialog : MonoBehaviour
{
    public Image[] mangaImages; // Assign 4 images in order
    public Image[] mangaImageTexts; // Assign 4 text backgrounds in order
    public string[] dialogTexts = {
        "First Manga Text",
        "Second Manga Text",
        "Third Manga Text",
        "Fourth Manga Text"
    };

    private TMP_Text[] textComponents;
    private int currentStep = 0;
    private bool isAnimating = false;

    private Vector2[] originalImagePositions;
    private Vector2[] originalTextPositions;
    private Vector3[] originalImageScales;
    private Vector3[] originalTextScales;

    void Awake()
    {
        int count = mangaImages.Length;
        textComponents = new TMP_Text[count];
        originalImagePositions = new Vector2[count];
        originalTextPositions = new Vector2[count];
        originalImageScales = new Vector3[count];
        originalTextScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            textComponents[i] = mangaImageTexts[i].GetComponentInChildren<TMP_Text>();
            textComponents[i].text = "";

            // Store original transforms
            originalImagePositions[i] = mangaImages[i].rectTransform.anchoredPosition;
            originalImageScales[i] = mangaImages[i].rectTransform.localScale;
            originalTextPositions[i] = mangaImageTexts[i].rectTransform.anchoredPosition;
            originalTextScales[i] = mangaImageTexts[i].rectTransform.localScale;

            mangaImages[i].gameObject.SetActive(false);
            mangaImageTexts[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            StartCoroutine(ShowNext());
        }
    }

    IEnumerator ShowNext()
    {
        if (currentStep >= mangaImages.Length * 2)
            yield break;

        isAnimating = true;

        int idx = currentStep / 2;
        bool isImage = currentStep % 2 == 0;

        // Restore original transform values
        mangaImages[idx].rectTransform.anchoredPosition = originalImagePositions[idx];
        mangaImages[idx].rectTransform.localScale = originalImageScales[idx];
        mangaImageTexts[idx].rectTransform.anchoredPosition = originalTextPositions[idx];
        mangaImageTexts[idx].rectTransform.localScale = originalTextScales[idx];

        // --- Remove 1-4 before showing 5-9 ---
        if (idx == 4 && isImage)
        {
            // Fade out TMP_Text first
            for (int i = 0; i < 4; i++)
            {
            TMP_Text txt = mangaImageTexts[i].GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                LeanTween.value(mangaImageTexts[i].gameObject, 1f, 0f, 0.2f)
                .setOnUpdate((float val) => {
                    Color c = txt.color;
                    c.a = val;
                    txt.color = c;
                });
            }
            }
            yield return new WaitForSeconds(0.13f);

            // Fade out text backgrounds and images
            for (int i = 0; i < 4; i++)
            {
            LeanTween.alpha(mangaImageTexts[i].rectTransform, 0, 0.2f);
            if (mangaImages[i].gameObject.activeSelf)
            {
                LeanTween.alpha(mangaImages[i].rectTransform, 0, 0.4f);
            }
            }
            yield return new WaitForSeconds(0.32f);

            for (int i = 0; i < 4; i++)
            {
            mangaImages[i].gameObject.SetActive(false);
            mangaImageTexts[i].gameObject.SetActive(false);
            }
        }

        if (isImage)
        {
            mangaImages[idx].gameObject.SetActive(true);

            // Keep color but set alpha to 0
            Color imgColor = mangaImages[idx].color;
            imgColor.a = 0;
            mangaImages[idx].color = imgColor;

            // Different enter animation for each image
            switch (idx)
            {
                case 0: // Fade in
                    LeanTween.alpha(mangaImages[idx].rectTransform, 1, 0.5f);
                    break;
                case 1: // Slide from left
                    mangaImages[idx].rectTransform.anchoredPosition = originalImagePositions[idx] + new Vector2(-500, 0);
                    LeanTween.move(mangaImages[idx].rectTransform, originalImagePositions[idx], 0.5f).setEaseOutBack();
                    LeanTween.alpha(mangaImages[idx].rectTransform, 1, 0.5f);
                    break;
                case 2: // Scale up
                    mangaImages[idx].rectTransform.localScale = Vector3.zero;
                    LeanTween.scale(mangaImages[idx].rectTransform, originalImageScales[idx], 0.5f).setEaseOutElastic();
                    LeanTween.alpha(mangaImages[idx].rectTransform, 1, 0.5f);
                    break;
                case 3: // Slide from bottom
                    mangaImages[idx].rectTransform.anchoredPosition = originalImagePositions[idx] + new Vector2(0, -500);
                    LeanTween.move(mangaImages[idx].rectTransform, originalImagePositions[idx], 0.5f).setEaseOutBack();
                    LeanTween.alpha(mangaImages[idx].rectTransform, 1, 0.5f);
                    break;
                default: // For 4-8, just fade in or use your preferred animation
                    LeanTween.alpha(mangaImages[idx].rectTransform, 1, 0.5f);
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            mangaImageTexts[idx].gameObject.SetActive(true);

            Color textBgColor = mangaImageTexts[idx].color;
            textBgColor.a = 0;
            mangaImageTexts[idx].color = textBgColor;

            LeanTween.alpha(mangaImageTexts[idx].rectTransform, 1, 0.3f);
            yield return new WaitForSeconds(0.2f);

            yield return StartCoroutine(TypeText(textComponents[idx], dialogTexts[idx], 0.03f));
        }

        currentStep++;
        isAnimating = false;

        // --- After last slide, wait for click, then fade out all images and texts ---
        if (currentStep >= mangaImages.Length * 2)
        {
            // Wait for player to click
            bool clicked = false;
            while (!clicked)
            {
                if (Input.GetMouseButtonDown(0))
                    clicked = true;
                yield return null;
            }

            // Fade out all images and text backgrounds
            for (int i = 0; i < mangaImages.Length; i++)
            {
                // Fade out the TMP_Text first
                TMP_Text txt = mangaImageTexts[i].GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    LeanTween.value(mangaImageTexts[i].gameObject, 1f, 0f, 0.3f)
                        .setOnUpdate((float val) =>
                        {
                            Color c = txt.color;
                            c.a = val;
                            txt.color = c;
                        });
                }
            }
            // Wait for text to fade out
            yield return new WaitForSeconds(0.18f);

            for (int i = 0; i < mangaImages.Length; i++)
            {
                if (mangaImages[i].gameObject.activeSelf)
                    LeanTween.alpha(mangaImages[i].rectTransform, 0, 0.3f);
                if (mangaImageTexts[i].gameObject.activeSelf)
                    LeanTween.alpha(mangaImageTexts[i].rectTransform, 0, 0.3f);
            }
            yield return new WaitForSeconds(0.35f);

            for (int i = 0; i < mangaImages.Length; i++)
            {
                mangaImages[i].gameObject.SetActive(false);
                mangaImageTexts[i].gameObject.SetActive(false);
            }

            // Move to next scene after last slide is done and fade out is complete
            SceneManager.LoadScene("Kantor-Quest");
            yield break;
        }
    }

    IEnumerator TypeText(TMP_Text textComp, string fullText, float delay)
    {
        textComp.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            textComp.text += fullText[i];
            yield return new WaitForSeconds(delay);
        }
    }
}
