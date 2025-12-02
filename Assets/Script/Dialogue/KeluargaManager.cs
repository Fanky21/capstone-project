using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeluargaManager : MonoBehaviour
{
    public static KeluargaManager Instance;

    [Header("Settings")]
    [SerializeField] private float minRequestInterval = 10f; // 10 menit
    [SerializeField] private float maxRequestInterval = 15f; // 15 menit
    [SerializeField] private int moneyAmount = 10000;

    [Header("Dialogue")]
    [SerializeField] private string characterName = "Keluarga";
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Button Controls")]
    [SerializeField] public GameObject buttonMenerima;
    [SerializeField] public GameObject buttonMenolak;
    [SerializeField] public GameObject buttonLanjutkan;

    private float nextRequestTime;
    private bool canMakeRequest = true;
    private int multiplier = 1;
    private int totalDebt = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Setup default dialogue lines jika kosong
        if (dialogueLines.Count == 0)
        {
            SetupDefaultDialogues();
        }

        ScheduleNextRequest();
    }

    private void Update()
    {
        if (canMakeRequest && Time.time >= nextRequestTime)
        {
            MakeMoneyRequest();
        }
    }

    private void ScheduleNextRequest()
    {
        float randomInterval = Random.Range(minRequestInterval * 60f, maxRequestInterval * 60f); // Convert to seconds
        nextRequestTime = Time.time + randomInterval;
        canMakeRequest = true;
    }

    public void MakeMoneyRequest()
    {
        canMakeRequest = false;

        // Create dialogue for money request
        Dialogue dialogue = new Dialogue();

        // Determine how much money is being requested
        int requestAmount = moneyAmount * multiplier;

        // Add dialogue lines from list
        foreach (DialogueLine line in dialogueLines)
        {
            DialogueLine newLine = new DialogueLine
            {
                name = characterName,
                utama_Dialogue = line.utama_Dialogue,
                line = line.line
            };
            dialogue.dialogueLines.Add(newLine);
        }

        // Start dialogue with choice buttons and pass the amount
        DialogueManager.Instance.StartDialogueWithChoices(dialogue, OnMoneyRequestChoice, requestAmount);
        
        // Show lanjutkan button initially (choice buttons hidden)
        HideChoiceButtons();
    }

    public void OnMoneyRequestChoice(bool accepted, int amount)
    {
        if (accepted)
        {
            // Player accepted - deduct money
            int deductAmount = moneyAmount * multiplier;
            Debug.Log($"Pemain menerima memberikan Rp{deductAmount:N0}");
            
            // Call money deduction dari Player.cs
            if (Player.instance != null)
            {
                // Check jika uang cukup
                if (Player.instance.uang >= deductAmount)
                {
                    Player.instance.SubtractUang(deductAmount);
                    
                    // Reset multiplier hanya jika pembayaran berhasil
                    multiplier = 1;
                    totalDebt = 0;
                }
                else
                {
                    // Uang tidak cukup - tetap di dialog tanpa melanjutkan
                    Debug.Log($"Uang tidak cukup!");
                    multiplier *= 2; // Tetap berlipat ganda
                    totalDebt = moneyAmount * multiplier;
                    ScheduleNextRequest();
                    return;
                }
            }
        }
        else
        {
            // Player refused - debt increases
            Debug.Log($"Pemain menolak - Utang menjadi 2x lipat!");
            multiplier *= 2;
            totalDebt = moneyAmount * multiplier;
        }

        // Schedule next request
        ScheduleNextRequest();
    }

    // Public methods to access debt info
    public int GetCurrentDebt()
    {
        return moneyAmount * multiplier;
    }

    public int GetMultiplier()
    {
        return multiplier;
    }

    public void ResetDebt()
    {
        multiplier = 1;
        totalDebt = 0;
    }

    // Public methods untuk button UI
    public void OnButtonTerima()
    {
        OnMoneyRequestChoice(true, GetCurrentDebt());
        HideChoiceButtons();
        EndDialogueUI();
        SoundManager.Instance.PlaySound2D("Button");
    }

    public void OnButtonTolak()
    {
        OnMoneyRequestChoice(false, GetCurrentDebt());
        HideChoiceButtons();
        EndDialogueUI();
        SoundManager.Instance.PlaySound2D("Button");
    }

    public void LanjutkanDialog()
    {
        if (DialogueManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
            // Check if there are more lines
            if (DialogueManager.Instance.HasMoreLines())
            {
                DialogueManager.Instance.DisplayNextDialogueLine();
                HideChoiceButtons(); // Keep continue button active
            }
            else
            {
                // Last dialogue line reached, show choice buttons
                ShowChoiceButtons();
            }
        }
    }

    private void ShowChoiceButtons()
    {
        if (buttonMenerima != null)
            buttonMenerima.SetActive(true);
        if (buttonMenolak != null)
            buttonMenolak.SetActive(true);
        if (buttonLanjutkan != null)
            buttonLanjutkan.SetActive(false);
        
        // Show requested amount when choices appear
        int currentDebt = GetCurrentDebt();
        DialogueManager.Instance.ShowRequestedAmount(currentDebt);
    }

    private void HideChoiceButtons()
    {
        if (buttonMenerima != null)
            buttonMenerima.SetActive(false);
        if (buttonMenolak != null)
            buttonMenolak.SetActive(false);
        if (buttonLanjutkan != null)
            buttonLanjutkan.SetActive(true);
    }

    private void EndDialogueUI()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue_Public();
        }
    }

    private void ShowInsufficientFundsDialogue(int requestAmount)
    {
        Dialogue dialogue = new Dialogue();

        // Add dialogue lines from list
        foreach (DialogueLine line in dialogueLines)
        {
            DialogueLine newLine = new DialogueLine
            {
                name = characterName,
                utama_Dialogue = line.utama_Dialogue,
                line = line.line
            };
            dialogue.dialogueLines.Add(newLine);
        }

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void SetupDefaultDialogues()
    {
        dialogueLines.Clear();
        
        DialogueLine line1 = new DialogueLine
        {
            name = characterName,
            utama_Dialogue = true,
            line = "Nak, keluarga butuh uang."
        };
        dialogueLines.Add(line1);

        DialogueLine line2 = new DialogueLine
        {
            name = characterName,
            utama_Dialogue = true,
            line = "Bisakah kamu memberikannya?"
        };
        dialogueLines.Add(line2);
    }
}
