using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapMovementAnimation : MonoBehaviour
{
    public GameObject mapCoverGameobject;
    public Image mapCover;

    void Start()
    {
        exitTransitionAnimation();
    }

    public void animationAndMoveMap(string mapname)
    {
        startTransitionAnimation();
        LeanTween.delayedCall(0.5f, () =>
        {
            SceneManager.LoadScene(mapname);
        });
    }
    
    public void startTransitionAnimation()
    {
        mapCoverGameobject.SetActive(true);

        RectTransform rect = mapCover.GetComponent<RectTransform>();
        Vector2 targetSize = new Vector2(4000f, 4000f);

        LeanTween.value(mapCover.gameObject, rect.sizeDelta, targetSize, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((Vector2 val) =>
            {
                rect.sizeDelta = val;
            })
            .setOnComplete(() =>
            {
                mapCoverGameobject.SetActive(true);
            });
    }

    public void exitTransitionAnimation()
    {
        Debug.Log("Exit Transition Animation");

        mapCoverGameobject.SetActive(true);

        RectTransform rect = mapCover.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(4000f, 4000f); // Set initial size

        Vector2 targetSize = Vector2.zero;

        LeanTween.value(mapCover.gameObject, rect.sizeDelta, targetSize, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((Vector2 val) =>
            {
            rect.sizeDelta = val;
            })
            .setOnComplete(() =>
            {
            mapCoverGameobject.SetActive(false);
            });
    }
}
