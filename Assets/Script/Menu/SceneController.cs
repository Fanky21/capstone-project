using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    [Header("Transition")]
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private GameObject transitionUI;

private void Awake()
{
    if (instance == null)
    {
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        if (transitionUI != null)
        {
            transitionUI.SetActive(true);

            // Pastikan canvas transisi selalu di depan
            Canvas c = transitionUI.GetComponent<Canvas>();
            if (c != null)
            {
                c.overrideSorting = true;
                c.sortingOrder = 100;
            }
        }
    }
    else
    {
        if (transitionUI != null)
            transitionUI.SetActive(false);

        Destroy(gameObject);
    }
}


    private void Start()
    {
        if (transitionUI != null)
            StartCoroutine(HideTransitionUIAfterDelay(2f));
    }

    private IEnumerator HideTransitionUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transitionUI.SetActive(false);
    }

    public void NextLevel()
    {
        StartCoroutine(LoadLevelByIndex(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadLevelByName(sceneName));
    }

    private IEnumerator LoadLevelByIndex(int index)
    {
        if (transitionUI != null)
            transitionUI.SetActive(true);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("End");

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(index);
    }

    private IEnumerator LoadLevelByName(string name)
{
    if (transitionUI != null)
        transitionUI.SetActive(true);

    if (transitionAnim != null)
        transitionAnim.SetTrigger("End");

    // Tunggu durasi animasi fade-out secara aman
    if (transitionAnim != null)
    {
        yield return new WaitForSeconds(
            transitionAnim.runtimeAnimatorController.animationClips[0].length
        );
    }
    else
    {
        yield return new WaitForSeconds(1f);
    }

    SceneManager.LoadScene(name);

    // Tunggu 1 frame biar scene baru siap
    yield return null;

    // Fade-in di scene baru
    if (transitionAnim != null)
        transitionAnim.SetTrigger("Start");

    // Tunggu animasi fade-in selesai, lalu matikan UI
    if (transitionAnim != null)
    {
        yield return new WaitForSeconds(
            transitionAnim.runtimeAnimatorController.animationClips[0].length
        );
    }
    else
    {
        yield return new WaitForSeconds(1f);
    }

    if (transitionUI != null)
        transitionUI.SetActive(false);
}

}
