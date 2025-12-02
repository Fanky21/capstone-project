using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public TextMeshProUGUI characterName;
    public TextMeshProUGUI fontUtama;
    public TextMeshProUGUI requestedAmountText;
    public GameObject dialogBox;

    private Queue<DialogueLine> lines;
    private DialogueLine currentLine;
    private TextMeshProUGUI activeFont;
    private string fullSentence;
    private Coroutine typingCoroutine;

    public bool isDialogueActive = false;
    private bool isTyping = false;
    public float typingSpeed = 0.02f;
    
    private System.Action<bool, int> onChoiceCallback;
    private int callbackAmount;
    private bool hasChoices = false;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        lines = new Queue<DialogueLine>();

        if (dialogBox != null)
            dialogBox.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.R))
        {
            if (isTyping)
            {
                ShowFullSentence();
            }
            else
            {
                DisplayNextDialogueLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        lines.Clear();
        dialogBox.SetActive(true);
        hasChoices = false;

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void StartDialogueWithChoices(Dialogue dialogue, System.Action<bool, int> onChoice, int amount = 0)
    {
        StartDialogue(dialogue);
        hasChoices = true;
        onChoiceCallback = onChoice;
        callbackAmount = amount;
        
        // Hide requested amount initially (will show when choices appear)
        if (requestedAmountText != null)
        {
            requestedAmountText.gameObject.SetActive(false);
        }
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            if (hasChoices)
            {
                EndDialogue();
            }
            else
            {
                EndDialogue();
            }
            return;
        }

        currentLine = lines.Dequeue();
        
        // Update character name - ensure it's displayed
        if (characterName != null && !string.IsNullOrEmpty(currentLine.name))
        {
            characterName.text = currentLine.name;
            characterName.gameObject.SetActive(true);
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(currentLine));
    }

    public bool HasMoreLines()
    {
        return lines.Count > 0;
    }

    public void ShowRequestedAmount(int amount)
    {
        if (requestedAmountText != null && amount > 0)
        {
            requestedAmountText.text = $"Biaya: Rp{amount:N0}";
            requestedAmountText.gameObject.SetActive(true);
        }
    }

    public void HideRequestedAmount()
    {
        if (requestedAmountText != null)
        {
            requestedAmountText.gameObject.SetActive(false);
        }
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        fontUtama.gameObject.SetActive(false);

        isTyping = true;

        if (dialogueLine.utama_Dialogue)
        {
            activeFont = fontUtama;
        }
        else if (dialogueLine.utama_Dialogue)
        {
            Debug.LogWarning("Dua font dipilih, gunakan font utama sebagai default.");
            activeFont = fontUtama;
        }
        else
        {
            Debug.LogWarning("Tidak ada font dipilih, gunakan font utama sebagai default.");
            activeFont = fontUtama;
        }

        activeFont.gameObject.SetActive(true);
        activeFont.text = "";
        fullSentence = dialogueLine.line;

        foreach (char letter in fullSentence)
        {
            activeFont.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void ShowFullSentence()
    {
        if (activeFont != null)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            activeFont.text = fullSentence;
            isTyping = false;
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        fontUtama.gameObject.SetActive(false);
        characterName.text = "";
        dialogBox.SetActive(false);
        hasChoices = false;
    }

    public void EndDialogue_Public()
    {
        EndDialogue();
    }

    private void OnContinueButtonClicked()
    {
        if (!hasChoices)
        {
            if (isTyping)
            {
                ShowFullSentence();
            }
            else
            {
                DisplayNextDialogueLine();
            }
        }
    }
}
