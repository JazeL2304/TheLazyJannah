using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class PhotoDialogueLine
{
    public string characterName = "JANNAH";
    [TextArea(2, 5)]
    public string sentence;
}

public class DialoguePhotoManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("Photo Dialogue Lines")]
    public PhotoDialogueLine[] photoDialogueLines = new PhotoDialogueLine[]
    {
        new PhotoDialogueLine { characterName = "JANNAH", sentence = "Oke, foto kartu sudah!" },
        new PhotoDialogueLine { characterName = "JANNAH", sentence = "Sekarang aku harus kembalikan kartu ini ke dompet." },
        new PhotoDialogueLine { characterName = "JANNAH", sentence = "Tekan [F] untuk memasukkan kartu ke dompet." },
        new PhotoDialogueLine { characterName = "JANNAH", sentence = "Lalu tekan [Q] untuk mengembalikan dompet ke lemari." }
    };

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool dialogueTriggered = false;

    void Start()
    {
        if (dialogueBox == null)
        {
            dialogueBox = GameObject.Find("DialogueBox");
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    void Update()
    {
        // CEK INPUT LEFT CLICK (Mouse Button 0)
        if (dialogueActive)
        {
            if (Input.GetMouseButtonDown(0))  // ← LEFT CLICK!
            {
                Debug.Log("[PhotoDialogue] LEFT CLICK PRESSED!");

                if (isTyping)
                {
                    // Skip typing animation
                    StopAllCoroutines();
                    dialogueText.text = photoDialogueLines[currentLineIndex].sentence;
                    isTyping = false;
                }
                else
                {
                    // Next line
                    NextLine();
                }

                PlayClickSound();
            }
        }
    }

    public void StartPhotoDialogue()
    {
        if (dialogueTriggered) return;

        dialogueTriggered = true;
        currentLineIndex = 0;
        dialogueActive = true;

        Debug.Log("[PhotoDialogue] Starting dialogue after photo...");
        Debug.Log("[PhotoDialogue] dialogueActive set to TRUE");

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentLineIndex < photoDialogueLines.Length)
        {
            PhotoDialogueLine line = photoDialogueLines[currentLineIndex];

            if (nameText != null)
            {
                nameText.text = line.characterName;
            }

            StopAllCoroutines();
            StartCoroutine(TypeSentence(line.sentence));

            Debug.Log($"[PhotoDialogue] Displaying line {currentLineIndex}: {line.sentence}");
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;

        Debug.Log($"[PhotoDialogue] Next line called. Current index: {currentLineIndex}");

        if (currentLineIndex < photoDialogueLines.Length)
        {
            DisplayLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[PhotoDialogue] Dialogue ended");
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void ResetDialogue()
    {
        dialogueTriggered = false;
        currentLineIndex = 0;
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[PhotoDialogue] Dialogue reset");
    }
}
