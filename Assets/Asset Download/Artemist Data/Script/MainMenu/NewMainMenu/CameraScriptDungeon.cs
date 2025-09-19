using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScriptDungeon : MonoBehaviour
{
    // Camera start and end positions
    public Vector3 startPosition = new Vector3(-22.8f, 4.1f, -10f);
    public Vector3 endPosition = new Vector3(-3, 1.3f, -11f);
    public float moveDuration;

    // Reference to the Canvas you want to move with the camera
    public Canvas canvasToMove;

    private Vector3 canvasOffset;

    void Start()
    {
        transform.position = startPosition;

        if (canvasToMove != null)
        {
            // Calculate the offset between the canvas and the camera at the start
            // Make the canvas a child of the camera so it follows the camera's movement
            canvasToMove.transform.SetParent(transform, worldPositionStays: true);
        }

        // Use LeanTween's setEase to make the movement linear and ensure duration is in seconds
        LeanTween.move(gameObject, endPosition, moveDuration).setEase(LeanTweenType.linear)
            .setOnComplete(() => {
                SceneManager.LoadScene("MainMenu");
            });
    }
}
