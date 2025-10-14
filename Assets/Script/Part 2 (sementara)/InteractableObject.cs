using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string objectName = "Object";
    public KeyCode interactionKey = KeyCode.E;
    public float maxInteractionDistance = 5f;

    [Header("UI Prompt")]
    public GameObject interactionPromptUI;
    public TMPro.TextMeshProUGUI promptText;

    [Header("Interaction Type")]
    public InteractionType interactionType;

    [Header("Wallet Specific Settings")]
    public QuestManager questManager;
    public int questIndex = 0;
    public int objectiveIndex = 1;
    public DialogueWalletManager dialogueWalletManager;

    [Header("Object Behavior")]
    public GameObject objectToHide;
    public GameObject objectToSpawn;
    public Transform spawnPoint;

    [Header("Hold Settings")]
    public bool holdAfterPickup = true;
    public ItemHolder itemHolder;

    private bool hasInteracted = false;

    public enum InteractionType
    {
        Wallet,
        Phone,
        Other
    }

    void Start()
    {
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (dialogueWalletManager == null)
        {
            dialogueWalletManager = FindObjectOfType<DialogueWalletManager>();
        }

        if (itemHolder == null)
        {
            itemHolder = FindObjectOfType<ItemHolder>();
        }

        // AUTO-FIND PROMPT UI (BARU - Support text only!)
        if (promptText == null)
        {
            // Cari TextMeshPro dengan nama InteractionPrompt
            TMPro.TextMeshProUGUI[] allTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text.name.Contains("InteractionPrompt"))
                {
                    promptText = text;
                    interactionPromptUI = text.gameObject;
                    Debug.Log("[InteractableObject] Found prompt text: " + text.name);
                    break;
                }
            }
        }

        // Hide prompt at start
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
        else if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    public void ShowPrompt()
    {
        if (hasInteracted) return;

        // Support both GameObject and TextMeshPro directly
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = "[E] Interact";
        }
        else if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        // Support both GameObject and TextMeshPro directly
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
        else if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    public void Interact()
    {
        if (hasInteracted) return;

        hasInteracted = true;

        Debug.Log(objectName + " di-interact!");

        HidePrompt();

        switch (interactionType)
        {
            case InteractionType.Wallet:
                PickupWallet();
                break;
            case InteractionType.Phone:
                PickupPhone();
                break;
            case InteractionType.Other:
                GenericInteraction();
                break;
        }
    }

    void PickupWallet()
    {
        Debug.Log("Wallet diambil!");

        if (holdAfterPickup && itemHolder != null && objectToHide != null)
        {
            // SAVE ORIGINAL DATA SEBELUM APA-APA! (PALING AMAN!)
            Vector3 originalPosition = objectToHide.transform.position;
            Quaternion originalRotation = objectToHide.transform.rotation;
            Vector3 originalScale = objectToHide.transform.localScale;

            Debug.Log("======== WALLET ORIGINAL DATA (BEFORE HOLD) ========");
            Debug.Log("Position: " + originalPosition);
            Debug.Log("Rotation: " + originalRotation.eulerAngles);
            Debug.Log("Scale: " + originalScale);
            Debug.Log("====================================================");

            // Hold di tangan (ini akan trigger save di ItemHolder, tapi kita override!)
            itemHolder.HoldObject(objectToHide);

            // OVERRIDE SEMUA DATA dengan yang kita save! (PENTING!)
            itemHolder.SetWalletOriginalData(originalPosition, originalRotation, originalScale);

            Debug.Log("[InteractableObject] Wallet dipegang & data saved!");
        }
        else
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }
        }

        // Spawn credit card
        if (objectToSpawn != null && spawnPoint != null)
        {
            Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

        // Complete objective
        if (questManager != null && !questManager.IsObjectiveComplete(questIndex, objectiveIndex))
        {
            questManager.CompleteCurrentObjective();
            Debug.Log("Objective 2 complete!");
        }

        // Trigger dialogue
        if (dialogueWalletManager != null)
        {
            Invoke("TriggerDialogue", 0.5f);
        }
    }

    void PickupPhone()
    {
        Debug.Log("Phone diambil!");
        // TODO: Implement phone pickup logic
    }

    void GenericInteraction()
    {
        Debug.Log("Generic interaction!");
    }

    void TriggerDialogue()
    {
        if (dialogueWalletManager != null)
        {
            dialogueWalletManager.StartWalletDialogue();
        }
    }

    // Reset untuk bisa diambil lagi
    public void ResetInteraction()
    {
        hasInteracted = false;
        Debug.Log("[InteractableObject] " + objectName + " reset - bisa diambil lagi!");
    }
}
