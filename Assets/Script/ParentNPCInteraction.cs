using UnityEngine;
using TMPro;
using System.Collections;

public class ParentNPCInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 3f;
    public GameObject interactionPrompt; // UI "[E] Bicara"

    [Header("Final Dialogue")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("Audio Settings")] // ← BARU - Section untuk audio
    public AudioClip clickSound; // ← BARU - Sound untuk click dialogue
    private AudioSource audioSource; // ← BARU - AudioSource component

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker = "PAPA";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("📝 FINAL HONEST DIALOGUE")]
    public DialogueLine[] finalDialogues = new DialogueLine[]
    {
        new DialogueLine { speaker = "PAPA", text = "Wah, kamarmu sudah rapi sekali Jannah!" },
        new DialogueLine { speaker = "MAMA", text = "Papa dan Mama bangga sama kamu." },
        new DialogueLine { speaker = "JANNAH", text = "Terima kasih Pa, Ma. Aku janji akan lebih bertanggung jawab." },
        new DialogueLine { speaker = "PAPA", text = "Itu yang Papa mau dengar. Ayo kita mulai dari awal lagi." },
        new DialogueLine { speaker = "MAMA", text = "Kami sayang kamu, Jannah." }
    };

    [Header("Good Ending Settings")]
    public Act2ChoiceManager choiceManager;

    private bool canInteract = false;
    private bool isInteracting = false;
    private GameObject player;
    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        player = GameObject.FindGameObjectWithTag("Player");

        if (choiceManager == null)
        {
            choiceManager = FindObjectOfType<Act2ChoiceManager>();
        }

        // ← BARU - Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[ParentNPC] EventSystem created!");
        }

        Debug.Log("[ParentNPC] Script initialized!");
    }

    void Update()
    {
        if (isInteracting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayClickSound(); // ← BARU - Play sound setiap click!

                Debug.Log("[ParentNPC] 🖱️ LEFT CLICK detected!");
                Debug.Log($"[ParentNPC] isTyping: {isTyping}, currentLine: {currentLine}/{finalDialogues.Length}");

                if (isTyping)
                {
                    StopAllCoroutines();
                    dialogueText.text = finalDialogues[currentLine].text;
                    isTyping = false;
                    Debug.Log("[ParentNPC] Typing skipped!");
                }
                else
                {
                    NextLine();
                }
            }

            return;
        }

        if (!canInteract) return;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                if (Input.GetKeyDown(interactKey))
                {
                    StartFinalDialogue();
                }
            }
            else
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
    }

    // ← BARU - Function untuk play click sound
    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void EnableInteraction()
    {
        canInteract = true;
        Debug.Log("[ParentNPC] ✅ Interaksi dengan parent NPC diaktifkan!");
    }

    void StartFinalDialogue()
    {
        isInteracting = true;
        currentLine = 0;

        Debug.Log("[ParentNPC] Starting final dialogue...");

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        TrashCleanupManager cleanupManager = FindObjectOfType<TrashCleanupManager>();
        if (cleanupManager != null && cleanupManager.questUI != null)
        {
            cleanupManager.questUI.SetActive(false);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            Debug.Log("[ParentNPC] DialogueBox activated!");
        }
        else
        {
            Debug.LogError("[ParentNPC] ❌ DialogueBox is NULL!");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[ParentNPC] ✅ Cursor unlocked & visible!");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log("[ParentNPC] Player movement disabled!");
            }
        }

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        DialogueLine line = finalDialogues[currentLine];

        if (dialogueNameText != null)
        {
            dialogueNameText.text = line.speaker;
        }

        foreach (char c in line.text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < finalDialogues.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndFinalDialogue();
        }
    }

    void EndFinalDialogue()
    {
        isInteracting = false;
        canInteract = false;

        Debug.Log("[ParentNPC] ✅ Final dialogue selesai! Menuju Good Ending...");

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            Debug.Log("[ParentNPC] Interaction prompt HIDDEN!");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
                Debug.Log("[ParentNPC] Player movement re-enabled!");
            }
        }

        if (choiceManager != null)
        {
            choiceManager.ShowGoodEndingAfterCleanup();
        }
        else
        {
            Debug.LogError("[ParentNPC] ❌ Act2ChoiceManager not found!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}