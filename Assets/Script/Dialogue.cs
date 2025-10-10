using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI nameTextComponent;
    public TextMeshProUGUI dialogueTextComponent;
    public GameObject dialogueBox;

    public DialogueLine[] lines;
    public float textSpeed = 0.05f;

    [Header("Pause Settings")]
    public int pauseAfterLineIndex = 2;

    [Header("Choice Settings")]
    public bool showChoiceAfterLine = false;
    public int showChoiceAtLineIndex = 7;
    public DialogueChoice dialogueChoice;

    [Header("Post-Choice Dialogue")]
    public int continueFromLineAfterChoice2 = 9;
    public int endLineAfterChoice2 = 10; // Line terakhir dialog setelah choice 2

    private int index;
    private bool isPaused = false;
    private bool isTyping = false;
    private bool isPostChoice2Dialogue = false; // Flag untuk track dialog setelah choice 2

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(1, 3)]
        public string sentence;
    }

    void Start()
    {
        if (nameTextComponent == null)
        {
            Debug.LogError("ERROR: Name Text Component belum diisi!");
            return;
        }

        if (dialogueTextComponent == null)
        {
            Debug.LogError("ERROR: Dialogue Text Component belum diisi!");
            return;
        }

        if (dialogueBox == null)
        {
            Debug.LogError("ERROR: Dialogue Box belum diisi!");
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("ERROR: Dialog Lines kosong!");
            return;
        }

        Debug.Log("Dialog Lines: " + lines.Length + " lines");

        dialogueTextComponent.text = string.Empty;
        StartDialogue();
    }

    void Update()
    {
        if (isPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueTextComponent.text = lines[index].sentence;
                isTyping = false;
            }
            else if (dialogueTextComponent.text == lines[index].sentence)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        index = 0;
        isPaused = false;
        isPostChoice2Dialogue = false;
        ShowDialogueBox();
        DisplayLine();
        Debug.Log("Dialog dimulai!");
    }

    public void ResumeDialogue()
    {
        if (isPaused)
        {
            Debug.Log("Resume dialog dari line " + (index + 1));
            isPaused = false;
            ShowDialogueBox();
            index++;
            DisplayLine();
        }
    }

    public void ContinueDialogueAfterChoice2()
    {
        if (continueFromLineAfterChoice2 >= 0 && continueFromLineAfterChoice2 < lines.Length)
        {
            Debug.Log("Dialog lanjutan setelah choice 2 - Line " + (continueFromLineAfterChoice2 + 1));
            index = continueFromLineAfterChoice2;
            isPostChoice2Dialogue = true; // Set flag
            ShowDialogueBox();
            DisplayLine();
        }
        else
        {
            Debug.LogWarning("Tidak ada dialog lanjutan. Check 'Continue From Line After Choice 2'");
        }
    }

    void DisplayLine()
    {
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        nameTextComponent.text = lines[index].characterName;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueTextComponent.text = string.Empty;

        foreach (char c in lines[index].sentence.ToCharArray())
        {
            dialogueTextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        // CEK: Jika sedang dialog post-choice 2, cek apakah sudah sampai end line
        if (isPostChoice2Dialogue && index >= endLineAfterChoice2)
        {
            Debug.Log("Dialog post-choice 2 selesai di line " + (index + 1));
            EndDialogueAfterChoice2();
            return;
        }

        if (index == pauseAfterLineIndex)
        {
            isPaused = true;
            HideDialogueBox();
            Debug.Log("Dialog PAUSE setelah line " + (index + 1));
            return;
        }

        if (showChoiceAfterLine && index == showChoiceAtLineIndex)
        {
            Debug.Log("Menampilkan pilihan setelah line " + (index + 1));
            HideDialogueBox();

            if (dialogueChoice != null)
            {
                dialogueChoice.ShowChoicePanel();
            }
            else
            {
                Debug.LogError("DialogueChoice belum di-set!");
            }
            return;
        }

        if (index < lines.Length - 1)
        {
            index++;
            DisplayLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        HideDialogueBox();
        Debug.Log("Dialog selesai!");
    }

    void EndDialogueAfterChoice2()
    {
        HideDialogueBox();
        isPostChoice2Dialogue = false;
        Debug.Log("Dialog setelah choice 2 selesai - Memulai stealth mission");

        // Notify GameManager bahwa dialog selesai
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnDialogueCompleteAfterChoice2();
        }
    }

    void ShowDialogueBox()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }
    }

    void HideDialogueBox()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public bool IsFinished()
    {
        return index >= lines.Length - 1 && !isTyping && !isPaused;
    }
}