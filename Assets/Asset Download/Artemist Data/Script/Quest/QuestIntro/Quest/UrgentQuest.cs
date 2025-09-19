using UnityEngine.UI;
using UnityEngine;

public class UrgentQuest : MonoBehaviour
{
    private NPCDialogManagerMaster npcDialogManager;
    [SerializeField] private PlayerMovement playerMovement;

    [System.Obsolete]
    void Awake()
    {

    }
    private bool secondDialog = false;

    [System.Obsolete]
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (secondDialog == false && other.CompareTag("Player"))
        {
            secondDialog = true;
            var playerMovement = other.gameObject.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                // Stop player movement before starting dialog
                playerMovement.StopMovement();
            }
            StartSecondDialog();
        }
    }

    [System.Obsolete]
    void StartSecondDialog()
    {
        if (npcDialogManager == null)
        {
            npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        }
        if (npcDialogManager != null)
        {
            // Daftarkan event sebelum mulai dialog
            npcDialogManager.onDialogFinished += HandleDialogFinished;
            
            npcDialogManager.InitiateStartDialog("Quest/MainLineQuest/Battle-UrgentDL");
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

