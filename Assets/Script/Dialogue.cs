using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI nameTextComponent;
    public TextMeshProUGUI dialogueTextComponent;
    public GameObject dialogueBox;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Dialogue Settings")]
    public DialogueLine[] lines;
    public float textSpeed = 0.05f;

    [Header("Pause Settings")]
    public int pauseAfterLineIndex = 2;

    [Header("Choice Settings")]
    public bool showChoiceAfterLine = false;
    public int showChoiceAtLineIndex = 7;
    public DialogueChoice dialogueChoice;

    [Header("End Game Settings")]
    public bool isLastDialogueAct1 = false;
    public GameObject endingCanvas;

    [Header("Post-Choice Dialogue")]
    public int continueFromLineAfterChoice2 = 9;
    public int endLineAfterChoice2 = 10;

    private int index;
    private bool isPaused = false;
    private bool isTyping = false;
    private bool isPostChoice2Dialogue = false;

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(1, 3)]
        public string sentence;
    }

    void Start()
    {
        Debug.Log("[Dialogue] ========== ACT 1 DIALOGUE START ==========");

        // VALIDASI COMPONENTS
        if (nameTextComponent == null)
        {
            Debug.LogError("[Dialogue] ❌ Name Text Component NULL!");
            return;
        }

        if (dialogueTextComponent == null)
        {
            Debug.LogError("[Dialogue] ❌ Dialogue Text Component NULL!");
            return;
        }

        if (dialogueBox == null)
        {
            Debug.LogError("[Dialogue] ❌ Dialogue Box NULL!");
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("[Dialogue] ❌ Dialog Lines KOSONG!");
            return;
        }

        Debug.Log($"[Dialogue] ✅ All components valid!");
        Debug.Log($"[Dialogue] Total lines: {lines.Length}");

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // CEK GAMEPROGRESSMANAGER
        if (GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;
            Debug.Log($"[Dialogue] Current Act: {currentAct}");

            if (currentAct != 1)
            {
                Debug.LogWarning($"[Dialogue] ⚠️ Not Act 1 (Current: {currentAct}) - Disabling dialogue!");
                this.enabled = false;
                return;
            }
        }
        else
        {
            Debug.LogWarning("[Dialogue] ⚠️ GameProgressManager not found!");
        }

        dialogueTextComponent.text = string.Empty;

        Debug.Log("[Dialogue] Starting dialogue in 0.5 seconds...");
        Invoke("StartDialogue", 0.5f); // Small delay untuk ensure scene fully loaded
    }

    void Update()
    {
        if (isPaused) return;

        if (dialogueBox != null && !dialogueBox.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

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

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueBox != null && dialogueBox.activeSelf;
    }

    public void StartDialogue()
    {
        Debug.Log("[Dialogue] ========== START DIALOGUE ==========");

        index = 0;
        isPaused = false;
        isPostChoice2Dialogue = false;

        ShowDialogueBox();
        DisplayLine();

        Debug.Log("[Dialogue] Dialogue started! Click to continue...");
    }

    public void ResumeDialogue()
    {
        if (isPaused)
        {
            Debug.Log("[Dialogue] Resume dialog dari line " + (index + 1));
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
            Debug.Log("[Dialogue] Dialog lanjutan setelah choice 2 - Line " + (continueFromLineAfterChoice2 + 1));
            index = continueFromLineAfterChoice2;
            isPostChoice2Dialogue = true;
            ShowDialogueBox();
            DisplayLine();
        }
        else
        {
            Debug.LogWarning("[Dialogue] Tidak ada dialog lanjutan. Check 'Continue From Line After Choice 2'");
        }
    }

    void DisplayLine()
    {
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        Debug.Log($"[Dialogue] Displaying line {index}: {lines[index].sentence}");

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
        if (isPostChoice2Dialogue && index >= endLineAfterChoice2)
        {
            Debug.Log("[Dialogue] Dialog post-choice 2 selesai di line " + (index + 1));
            EndDialogueAfterChoice2();
            return;
        }

        if (index == pauseAfterLineIndex)
        {
            isPaused = true;
            HideDialogueBox();
            Debug.Log("[Dialogue] Dialog PAUSE setelah line " + (index + 1));
            return;
        }

        if (showChoiceAfterLine && index == showChoiceAtLineIndex)
        {
            Debug.Log("[Dialogue] Menampilkan pilihan setelah line " + (index + 1));
            HideDialogueBox();

            if (dialogueChoice != null)
            {
                dialogueChoice.ShowChoicePanel();
            }
            else
            {
                Debug.LogError("[Dialogue] DialogueChoice belum di-set!");
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
        Debug.Log("[Dialogue] Dialog selesai!");
    }

    void EndDialogueAfterChoice2()
    {
        HideDialogueBox();
        isPostChoice2Dialogue = false;
        Debug.Log("[Dialogue] Dialog setelah choice 2 selesai - Memulai stealth mission");

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnDialogueCompleteAfterChoice2();
        }
    }

    public void ShowEndingCanvas()
    {
        Debug.Log("[Dialogue] ✅ ShowEndingCanvas() called!");

        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);
            Debug.Log("[Dialogue] 🎬 Canvas Act 1 End MUNCUL!");
        }
        else
        {
            Debug.LogError("[Dialogue] ❌ endingCanvas belum di-assign di Inspector!");
        }
    }

    void ShowDialogueBox()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            Debug.Log("[Dialogue] ✅ Dialogue Box SHOWN!");

            // Unlock cursor untuk bisa klik
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogError("[Dialogue] ❌ Cannot show dialogue box - it's NULL!");
        }
    }

    void HideDialogueBox()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
            Debug.Log("[Dialogue] Dialogue Box hidden");
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