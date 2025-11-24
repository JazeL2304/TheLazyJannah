using UnityEngine;
using TMPro;
using System.Collections;

public class Act2DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    public string characterName = "JANNAH";
    public float textSpeed = 0.05f;

    [Header("⚙️ Trigger Settings")]
    public float startDelay = 0.5f;

    [Header("🔊 Audio Settings")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    [Header("Act 2 Dialogue")]
    [TextArea(3, 10)]
    public string[] act2Dialogues;

    [Header("🛑 Pause Settings")]
    public int pauseAfterLineIndex = 1; // Pause setelah line ke-2 (index 1)

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isPaused = false; // ✅ BARU - Untuk pause system

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Cek apakah ini Act 2
        if (GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;

            if (currentAct == 2)
            {
                if (dialogueBox != null)
                {
                    dialogueBox.SetActive(false);
                }

                StartCoroutine(AutoStartDialogue());
            }
            else
            {
                Debug.Log("[Act2Dialogue] Not Act 2 - Script disabled.");
                this.enabled = false;
            }
        }
    }

    IEnumerator AutoStartDialogue()
    {
        yield return new WaitForSeconds(startDelay);
        StartDialogue();
    }

    void Update()
    {
        if (!dialogueActive || isPaused) return; // ✅ Stop update kalau paused

        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = act2Dialogues[currentLine];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        dialogueActive = true;
        isPaused = false;
        currentLine = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = characterName;
        }

        if (act2Dialogues != null && act2Dialogues.Length > 0)
        {
            StartCoroutine(TypeLine());
        }

        Debug.Log("[Act2Dialogue] Dialogue started!");
    }

    // ✅ FUNCTION BARU - Resume dialogue yang di-pause
    public void ResumeDialogue()
    {
        if (isPaused)
        {
            Debug.Log("[Act2Dialogue] ✅ Resuming dialogue from line " + (currentLine + 1));
            isPaused = false;
            dialogueActive = true;

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }

            currentLine++; // Lanjut ke line berikutnya
            StartCoroutine(TypeLine());
        }
        else
        {
            Debug.LogWarning("[Act2Dialogue] ⚠️ Dialogue is not paused!");
        }
    }

    // ✅ FUNCTION BARU - Cek apakah dialogue sedang paused
    public bool IsPaused()
    {
        return isPaused;
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in act2Dialogues[currentLine].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        // ✅ CEK PAUSE - Pause setelah line tertentu
        if (currentLine == pauseAfterLineIndex)
        {
            isPaused = true;
            dialogueActive = false;

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }

            Debug.Log("[Act2Dialogue] 🛑 Dialogue PAUSED after line " + (currentLine + 1));
            return; // Stop di sini, tunggu resume
        }

        currentLine++;
        if (currentLine < act2Dialogues.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueActive = false;
        isPaused = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[Act2Dialogue] Dialogue finished!");
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
