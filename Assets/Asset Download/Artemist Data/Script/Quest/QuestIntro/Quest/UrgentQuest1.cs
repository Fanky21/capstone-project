using UnityEngine.UI;
using UnityEngine;

public class UrgentQuest1 : MonoBehaviour
{
    private NPCDialogManagerMaster npcDialogManager;
    [SerializeField] private PlayerMovement playerMovement;

    [System.Obsolete]
    void Awake()
    {

    }
    private bool thirdDialog = false;

    [System.Obsolete]
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (thirdDialog == false && other.CompareTag("Player"))
        {
            thirdDialog = true;
            var playerMovement = other.gameObject.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                // Stop player movement before starting dialog
                playerMovement.StopMovement();
            }
            StartThirdDialog();
        }
    }

    [System.Obsolete]
    void StartThirdDialog()
    {
        if (npcDialogManager == null)
        {
            npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        }
        if (npcDialogManager != null)
        {
            // Daftarkan event sebelum mulai dialog
            npcDialogManager.onDialogFinished += HandleDialogFinished;

            npcDialogManager.InitiateStartDialog("Quest/MainLineQuest/Battle-UrgentDL1");
        }
    }
    private void HandleDialogFinished()
    {
        Debug.Log("Dialog finished, allowing movement.");
        if (playerMovement != null)
        {
            playerMovement.AllowMovement();
        }
        else
        {
            Debug.LogWarning("PlayerMovement is null.");
        }
    }
}

