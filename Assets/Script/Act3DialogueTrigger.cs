using UnityEngine;

public class Act3DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Manager Reference")]
    public Act3DialogueManager dialogueManager;

    [Header("Trigger Settings")]
    public bool triggerOnce = true; // Hanya trigger sekali
    public bool requireInteraction = false; // Perlu tekan tombol atau auto?
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI Prompt (Optional)")]
    public GameObject interactionPrompt; // UI "[E] Bicara"

    private bool hasTriggered = false;
    private bool playerInRange = false;

    void Start()
    {
        // Auto-detect dialogue manager jika tidak di-assign
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Act3DialogueManager>();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Jika perlu interaksi dan player di range
        if (requireInteraction && playerInRange && !hasTriggered)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TriggerDialogue();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!hasTriggered)
            {
                if (requireInteraction)
                {
                    // Show prompt
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);
                    }
                }
                else
                {
                    // Auto trigger
                    TriggerDialogue();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    void TriggerDialogue()
    {
        if (hasTriggered && triggerOnce) return;

        hasTriggered = true;

        if (dialogueManager != null)
        {
            dialogueManager.TriggerDialogue();
            Debug.Log("[Act3Trigger] Dialogue triggered!");
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}