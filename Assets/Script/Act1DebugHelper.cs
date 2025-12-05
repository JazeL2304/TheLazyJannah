using UnityEngine;

public class Act1DebugHelper : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== ACT 1 DEBUG INFO ==========");

        // Check GameProgressManager
        if (GameProgressManager.Instance != null)
        {
            Debug.Log($"✅ GameProgressManager found!");
            Debug.Log($"   Current Act: {GameProgressManager.Instance.currentAct}");
            Debug.Log($"   Current Day: {GameProgressManager.Instance.currentDay}");
        }
        else
        {
            Debug.LogError("❌ GameProgressManager.Instance is NULL!");
        }

        // Check Dialogue Manager
        Dialogue dialogueManager = FindObjectOfType<Dialogue>();
        if (dialogueManager != null)
        {
            Debug.Log($"✅ Dialogue Manager found on: {dialogueManager.gameObject.name}");
            Debug.Log($"   Dialogue Box assigned: {(dialogueManager.dialogueBox != null ? "YES (" + dialogueManager.dialogueBox.name + ")" : "NO")}");
            Debug.Log($"   Name Text assigned: {(dialogueManager.nameTextComponent != null ? "YES" : "NO")}");
            Debug.Log($"   Dialogue Text assigned: {(dialogueManager.dialogueTextComponent != null ? "YES" : "NO")}");
            Debug.Log($"   Total lines: {(dialogueManager.lines != null ? dialogueManager.lines.Length.ToString() : "NULL")}");

            // Check if dialogue box is active
            if (dialogueManager.dialogueBox != null)
            {
                Debug.Log($"   Dialogue Box active in hierarchy: {dialogueManager.dialogueBox.activeInHierarchy}");
                Debug.Log($"   Dialogue Box active self: {dialogueManager.dialogueBox.activeSelf}");
            }
        }
        else
        {
            Debug.LogError("❌ Dialogue Manager NOT FOUND in scene!");
        }

        // Check BlinkController
        BlinkController blink = FindObjectOfType<BlinkController>();
        if (blink != null)
        {
            Debug.Log($"✅ BlinkController found - mata akan membuka");
        }

        Debug.Log("=====================================");
    }

    void Update()
    {
        // Manual trigger dengan tombol T untuk testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Debug] T key pressed - Manual start dialogue!");
            Dialogue dm = FindObjectOfType<Dialogue>();
            if (dm != null)
            {
                dm.StartDialogue();
            }
        }

        // Check dialogue box status dengan tombol Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[Debug] Y key pressed - Checking dialogue box status...");
            Dialogue dm = FindObjectOfType<Dialogue>();
            if (dm != null && dm.dialogueBox != null)
            {
                Debug.Log($"   Dialogue Box active: {dm.dialogueBox.activeSelf}");
                Debug.Log($"   Is dialogue active: {dm.IsDialogueActive()}");
            }
        }
    }
}