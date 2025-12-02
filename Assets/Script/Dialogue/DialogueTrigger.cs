using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
    

[System.Serializable]
public class DialogueLine
{
    public string name;

    [Tooltip("Centang jika ini adalah dialog utama (text biasa)")]
    public bool utama_Dialogue;

    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private bool playerInRange = false;
    private bool dialogueStarted = false;
    private Renderer quadRenderer;

    private void Start()
    {
        quadRenderer = GetComponent<Renderer>();
        if (quadRenderer != null)
            quadRenderer.enabled = false;
    }

    private void Update()
    {
        if (playerInRange && !dialogueStarted && !DialogueManager.Instance.isDialogueActive && Input.GetKeyDown(KeyCode.F))
        {
            DialogueManager.Instance.StartDialogue(dialogue);
            dialogueStarted = true;
        }

        if (dialogueStarted && !DialogueManager.Instance.isDialogueActive)
        {
            dialogueStarted = false;
        }
    }
} 