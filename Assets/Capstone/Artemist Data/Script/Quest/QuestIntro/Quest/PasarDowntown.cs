using UnityEngine.UI;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject backgroundImage;
    public Image backgroundImageComponent;
    private NPCDialogManagerMaster npcDialogManager;
    private bool firstDialog = false;
    private PlayerMovement playerMovement;

    [System.Obsolete]
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!firstDialog && other.CompareTag("Player"))
        {
            firstDialog = true;
            playerMovement = other.gameObject.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                // Hentikan pergerakan sebelum mulai dialog
                playerMovement.StopMovement();
            }
            StartFirstDialog();
        }
    }

    [System.Obsolete]
    void StartFirstDialog()
    {
        if (npcDialogManager == null)
        {
            npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        }
        if (npcDialogManager != null)
        {
            // Daftarkan event sebelum mulai dialog
            npcDialogManager.onDialogFinished += HandleDialogFinished;

            npcDialogManager.InitiateStartDialog("Quest/MainLineQuest/Chapter2-outsidehouse");
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
