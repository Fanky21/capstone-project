using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScript : MonoBehaviour
{
    // Camera start and end positions
    public Vector3 startPosition = new Vector3(-10f, 4.8f, -10f);
    public Vector3 endPosition = new Vector3(-25f, 4.8f, -10f);
    public float moveDuration;

    // Reference to the Canvas you want to move with the camera
    public Canvas canvasToMove;

    private Vector3 canvasOffset;

    void Start()
    {
        transform.position = startPosition;

        if (canvasToMove != null)
            canvasToMove.transform.SetParent(transform, worldPositionStays: true);

        // Use LeanTween's setEase to make the movement linear and ensure duration is in seconds
        LeanTween.move(gameObject, endPosition, moveDuration).setEase(LeanTweenType.linear)
            .setOnComplete(() => {
                SceneManager.LoadScene("Dungeon-MainMenu");
            });
    }
}
