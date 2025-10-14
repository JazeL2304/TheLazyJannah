using UnityEngine;
using System.Collections;
using TMPro;  // PENTING!

public class DialogueWalletManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("Wallet Dialogue Lines")]
    public DialogueLine[] walletDialogueLines;

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
                dialogueText.text = walletDialogueLines[currentLineIndex].sentence;
                isTyping = false;
            }
            else if (dialogueText.text == walletDialogueLines[currentLineIndex].sentence)
            {
                NextLine();
            }
        }
    }

    public void StartWalletDialogue()
    {
        Debug.Log("Dialog setelah ambil dompet dimulai!");

        dialogueActive = true;
        currentLineIndex = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentLineIndex >= walletDialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        nameText.text = walletDialogueLines[currentLineIndex].characterName;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in walletDialogueLines[currentLineIndex].sentence.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        if (currentLineIndex < walletDialogueLines.Length - 1)
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
        Debug.Log("Dialog setelah ambil dompet selesai!");

        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
