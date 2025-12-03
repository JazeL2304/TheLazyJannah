using UnityEngine;

public class DoorInteractionBlocker : MonoBehaviour
{
    [Header("References")]
    public DoorInteraction[] doorsToBlock; // Array pintu yang akan di-block
    public Act3DialogueManager dialogueManager;
    public Act3DoorEventManager doorEventManager;
    public Act3FightManager fightManager;

    private bool dialogueActive = false;
    private bool fightActive = false;

    void Start()
    {
        // Auto-detect all doors if not assigned
        if (doorsToBlock == null || doorsToBlock.Length == 0)
        {
            doorsToBlock = FindObjectsOfType<DoorInteraction>();
            Debug.Log($"[DoorBlocker] Auto-detected {doorsToBlock.Length} doors");
        }

        // Auto-detect managers
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Act3DialogueManager>();
        }

        if (doorEventManager == null)
        {
            doorEventManager = FindObjectOfType<Act3DoorEventManager>();
        }

        if (fightManager == null)
        {
            fightManager = FindObjectOfType<Act3FightManager>();
        }
    }

    void Update()
    {
        // Check if dialogue active
        bool wasDialogueActive = dialogueActive;
        dialogueActive = IsDialogueActive();

        // Check if fight active (bisa ditambahkan nanti)
        // fightActive = IsFightActive();

        // If dialogue just started, block doors
        if (dialogueActive && !wasDialogueActive)
        {
            BlockAllDoors();
        }
        // If dialogue just ended, unblock doors
        else if (!dialogueActive && wasDialogueActive)
        {
            // JANGAN unblock door - biarkan tetap block setelah event!
            // UnblockAllDoors();
        }
    }

    bool IsDialogueActive()
    {
        // Check Act3DialogueManager
        if (dialogueManager != null && dialogueManager.gameObject.activeInHierarchy)
        {
            // Cek private field "dialogueActive" via reflection atau cek dialogue box
            GameObject dialogueBox = dialogueManager.dialogueBox;
            if (dialogueBox != null && dialogueBox.activeSelf)
            {
                return true;
            }
        }

        // Check Act3DoorEventManager dialogue
        if (doorEventManager != null)
        {
            GameObject dialogueBox = doorEventManager.dialogueBox;
            if (dialogueBox != null && dialogueBox.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    void BlockAllDoors()
    {
        foreach (DoorInteraction door in doorsToBlock)
        {
            if (door != null && door.interactionUI != null)
            {
                door.interactionUI.SetActive(false);
            }
        }

        Debug.Log("[DoorBlocker] ✅ All doors blocked!");
    }

    void UnblockAllDoors()
    {
        foreach (DoorInteraction door in doorsToBlock)
        {
            if (door != null && door.interactionUI != null)
            {
                // Cek apakah player masih di trigger zone
                // (optional - bisa diabaikan kalau mau simple)
                door.interactionUI.SetActive(false); // Keep disabled untuk safety
            }
        }

        Debug.Log("[DoorBlocker] Doors unblocked (kept disabled for safety)");
    }

    // Function untuk permanently block door setelah event
    public void PermanentlyBlockDoor(DoorInteraction door)
    {
        if (door != null)
        {
            if (door.interactionUI != null)
            {
                door.interactionUI.SetActive(false);
            }

            // Disable collider agar tidak bisa di-trigger lagi
            Collider col = door.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            Debug.Log($"[DoorBlocker] Door {door.gameObject.name} permanently blocked!");
        }
    }
}