using UnityEngine;
using UnityEngine.UI;

public class ErikOfficeIntro : MonoBehaviour
{
    public GameObject backgroundImage;
    public Image backgroundImageComponent;
    public GameObject restrictCharacterMovement;

    private NPCDialogManagerMaster npcDialogManager;
    [SerializeField] private PlayerMovement_Indoor playerMovementIndoor;

    private bool isSecondDialogTriggered = false;
    public Collider2D introTriggerZone;

    // [System.Obsolete]
    // public void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player") && isSecondDialogTriggered == false)
    //     {
    //         // Disable the Player_MovementIndoor script on the Player GameObject
    //         var playerMovement = other.gameObject.GetComponent<PlayerMovement_Indoor>();
    //         if (playerMovement != null)
    //         {
    //             playerMovement.enabled = false;
    //         }

    //         StartSecondDialog();
    //         isSecondDialogTriggered = true;
    //     }
    // }

    [System.Obsolete]
    void Awake()
    {
        if (backgroundImageComponent != null && backgroundImage != null)
        {
            backgroundImage.SetActive(true);
            backgroundImageComponent.color = new Color(
                backgroundImageComponent.color.r,
                backgroundImageComponent.color.g,
                backgroundImageComponent.color.b,
                0f
            );
            LeanTween.value(gameObject, 1f, 0f, 4f)
                .setOnUpdate((float val) =>
                {
                    var c = backgroundImageComponent.color;
                    c.a = val;
                    backgroundImageComponent.color = c;
                })
                .setOnComplete(() =>
                {
                    if (playerMovementIndoor != null)
                    {
                        playerMovementIndoor.StopMovement();
                    }

                    backgroundImage.SetActive(false);
                    StartIntroDialog();

                    if (playerMovementIndoor != null)
                    {
                        playerMovementIndoor.AllowMovement();
                    }
                    playerMovementIndoor.AllowMovement();
                });
        }
    }

    [System.Obsolete]
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isSecondDialogTriggered == false)
        {
            // Use StopMovement instead of disabling the script
            var playerMovement = other.gameObject.GetComponent<PlayerMovement_Indoor>();
            playerMovement.StopMovement();

            Debug.Log("Player entered the trigger zone for second dialog.");

            StartSecondDialog();
            isSecondDialogTriggered = true;
        }
    }

    [System.Obsolete]
    void StartIntroDialog()
    {
        npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        npcDialogManager.InitiateStartDialog("Quest/MainLineQuest/intro");
    }

    [System.Obsolete]
    void StartSecondDialog()
    {
        npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        npcDialogManager.InitiateStartDialog("Quest/MainLineQuest/Chapter2-insidehouse");
    }
}
