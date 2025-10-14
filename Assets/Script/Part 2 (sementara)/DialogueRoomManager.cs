using UnityEngine;
using System.Collections;

public class DialogueRoomManager : MonoBehaviour
{
    [Header("Dialogue System")]
    public GameObject dialogueBox;
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("Room Dialogue Lines")]
    public DialogueLine[] roomDialogueLines;

    [Header("Quest Integration")]
    public QuestManager questManager;
    public int objectiveToComplete = 0;  // Objective 1

    [Header("Door Settings")]
    public DoorInteraction door;
    public float doorCloseDelay = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(1, 3)]
        public string sentence;
    }

    void Start()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = roomDialogueLines[currentLineIndex].sentence;
                isTyping = false;
            }
            else if (dialogueText.text == roomDialogueLines[currentLineIndex].sentence)
            {
                NextLine();
            }
        }
    }

    public void StartRoomDialogue()
    {
        Debug.Log("Dialog kamar dimulai!");

        dialogueActive = true;
        currentLineIndex = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // Lock cursor saat dialog
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentLineIndex >= roomDialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        nameText.text = roomDialogueLines[currentLineIndex].characterName;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in roomDialogueLines[currentLineIndex].sentence.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        if (currentLineIndex < roomDialogueLines.Length - 1)
        {
            currentLineIndex++;
            DisplayLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        Debug.Log("Dialog kamar selesai!");

        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Unlock cursor kembali
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Complete Objective 1 & Start Objective 2
        if (questManager != null && !questManager.IsObjectiveComplete(0, objectiveToComplete))
        {
            questManager.CompleteCurrentObjective();
            Debug.Log("Objective 1 complete → Objective 2 dimulai!");
        }

        // Close door setelah dialog
        if (door != null)
        {
            Invoke("CloseDoor", doorCloseDelay);
        }
    }

    void CloseDoor()
    {
        if (door != null && door.isOpen)
        {
            door.InteractWithDoor();
            Debug.Log("Pintu kamar tertutup otomatis.");
        }
    }
}
