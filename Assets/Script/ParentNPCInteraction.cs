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
    public Act2ChoiceManager choiceManager; // Reference untuk trigger good ending

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

        // PASTIKAN ADA EVENTSYSTEM (untuk detect click)
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
        // Handle dialogue progression dengan LEFT CLICK
        if (isInteracting)
        {
            // DEBUG LOG
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("[ParentNPC] 🖱️ LEFT CLICK detected!");
                Debug.Log($"[ParentNPC] isTyping: {isTyping}, currentLine: {currentLine}/{finalDialogues.Length}");
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // Skip typing animation
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

            // Skip rest of Update - jangan check interaction lagi
            return;
        }

        if (!canInteract) return;

        // Check distance to player
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                // Show prompt
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                // Check interact key
                if (Input.GetKeyDown(interactKey))
                {
                    StartFinalDialogue();
                }
            }
            else
            {
                // Hide prompt
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
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

        // Hide prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Hide Quest UI juga (kalau masih muncul)
        TrashCleanupManager cleanupManager = FindObjectOfType<TrashCleanupManager>();
        if (cleanupManager != null && cleanupManager.questUI != null)
        {
            cleanupManager.questUI.SetActive(false);
        }

        // Show dialogue box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            Debug.Log("[ParentNPC] DialogueBox activated!");
        }
        else
        {
            Debug.LogError("[ParentNPC] ❌ DialogueBox is NULL!");
        }

        // UNLOCK CURSOR & MAKE VISIBLE - PENTING!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[ParentNPC] ✅ Cursor unlocked & visible!");

        // DISABLE PLAYER MOVEMENT
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

        // Start typing
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

        Debug.Log("[ParentNPC] ✅ Final dialogue selesai! Menuju Good Ending...");

        // Hide dialogue box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // RE-ENABLE PLAYER MOVEMENT (kalau perlu)
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

        // Trigger Good Ending via Act2ChoiceManager
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
        // Visualisasi interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}