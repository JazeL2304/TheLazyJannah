using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// GAME MANAGER - ACT 1 ONLY
/// Handles choice system and endings
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("🎮 ACT VALIDATION")]
    [Tooltip("Only active in Act 1")]
    public bool onlyInAct1 = true;

    [Header("Happy Ending Settings")]
    public GameObject happyEndingPanel;
    public GameObject mainMenuButton;
    public GameObject choicePanel;
    public GameObject dialogueBox;

    [Header("Stealth Mission Settings")]
    public GameObject stealthQuestUI;
    public GameObject ibuNPC;
    public GameObject bapakNPC;
    public Dialogue dialogueManager;
    public QuestManager questManager;
    public StealthManager stealthManager;

    [Header("Post-Choice 2 Settings")]
    public bool showDialogueAfterChoice2 = true;
    public float delayBeforeDialogue = 1f;

    private int playerChoice = 0;
    private bool waitingForDialogueComplete = false;

    void Start()
    {
        // ✅ VALIDASI ACT - HANYA AKTIF DI ACT 1!
        if (onlyInAct1 && GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;

            if (currentAct != 1)
            {
                Debug.Log($"[GameManager] Not Act 1 (Current: Act {currentAct}) - Disabling script!");
                this.enabled = false;
                return;
            }
        }

        // Prevent duplicates
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1)
        {
            Debug.LogWarning("[GameManager] Duplicate found! Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        // Hide panels at start
        if (happyEndingPanel != null) happyEndingPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (stealthQuestUI != null) stealthQuestUI.SetActive(false);

        // NPCs start hidden (will be activated by quest)
        if (ibuNPC != null) ibuNPC.SetActive(false);
        if (bapakNPC != null) bapakNPC.SetActive(false);

        // Auto-detect QuestManager
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
            if (questManager != null)
            {
                Debug.Log("[GameManager] QuestManager auto-detected!");
            }
        }

        // Auto-detect StealthManager
        if (stealthManager == null)
        {
            stealthManager = FindObjectOfType<StealthManager>();
            if (stealthManager != null)
            {
                Debug.Log("[GameManager] StealthManager auto-detected!");
            }
        }

        Debug.Log("[GameManager] ✅ Initialized for Act 1!");
    }

    /// <summary>
    /// Called by DialogueChoice when player makes a choice
    /// </summary>
    public void OnPlayerChoice(int choice)
    {
        playerChoice = choice;
        Debug.Log($"[GameManager] ===== PLAYER CHOICE: {choice} =====");

        if (choice == 1)
        {
            Debug.Log("[GameManager] Player chose: JANGAN CURI (Good Ending)");
            ShowHappyEnding();
        }
        else if (choice == 2)
        {
            Debug.Log("[GameManager] Player chose: AMBIL KARTU KREDIT (Stealth Route)");
            StartCoroutine(StartStealthMissionSequence());
        }
    }

    /// <summary>
    /// Show Happy Ending panel (Choice 1)
    /// </summary>
    void ShowHappyEnding()
    {
        Debug.Log("[GameManager] >>> ENDING 1/3: HAPPY ENDING <<<");

        // Force close all other panels
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Disable DialogueManager script
        if (dialogueManager != null)
        {
            dialogueManager.gameObject.SetActive(false);
        }

        // Show happy ending panel
        if (happyEndingPanel != null)
        {
            happyEndingPanel.SetActive(true);
            Debug.Log("[GameManager] ✅ Happy Ending Panel displayed!");
        }
        else
        {
            Debug.LogError("[GameManager] ❌ Happy Ending Panel is NULL!");
        }

        // Show cursor for buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
        }

        Debug.Log("[GameManager] Game ended with Happy Ending!");
    }

    /// <summary>
    /// Start stealth mission sequence (Choice 2)
    /// </summary>
    IEnumerator StartStealthMissionSequence()
    {
        Debug.Log("[GameManager] >>> Starting stealth mission sequence...");

        // Hide choice panel
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // Show post-choice dialogue
        if (showDialogueAfterChoice2 && dialogueManager != null)
        {
            waitingForDialogueComplete = true;
            dialogueManager.ContinueDialogueAfterChoice2();
            Debug.Log("[GameManager] Post-choice dialogue started - waiting for completion...");

            // Wait for dialogue to finish
            while (waitingForDialogueComplete)
            {
                yield return null;
            }

            Debug.Log("[GameManager] ✅ Dialogue complete! Starting stealth mission...");
        }

        // Start stealth mission AFTER dialogue
        StartStealthMission();
    }

    /// <summary>
    /// Called by Dialogue.cs when post-choice dialogue finishes
    /// </summary>
    public void OnDialogueCompleteAfterChoice2()
    {
        Debug.Log("[GameManager] 📢 Received notification: Post-choice dialogue complete!");
        waitingForDialogueComplete = false;
    }

    /// <summary>
    /// Activate stealth mission components
    /// </summary>
    void StartStealthMission()
    {
        Debug.Log("[GameManager] === MISI STEALTH DIMULAI ===");

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Start quest via QuestManager
        if (questManager != null)
        {
            questManager.StartQuest(0); // Quest index 0 = MISI STEALTH
            Debug.Log("[GameManager] ✅ Quest started via QuestManager!");
        }
        else
        {
            Debug.LogWarning("[GameManager] ⚠️ QuestManager not found! Falling back to manual UI...");

            // Fallback: Show UI manually
            if (stealthQuestUI != null)
            {
                stealthQuestUI.SetActive(true);
            }
        }

        // Activate NPCs
        if (ibuNPC != null)
        {
            ibuNPC.SetActive(true);

            // ✅ ENABLE VISION CONE!
            NPCVisionCone ibuVision = ibuNPC.GetComponent<NPCVisionCone>();
            if (ibuVision != null)
            {
                ibuVision.enabled = true;
                Debug.Log("[GameManager] ✅ Ibu vision cone ENABLED!");
            }

            Debug.Log("[GameManager] ✅ Ibu NPC activated!");
        }
        else
        {
            Debug.LogWarning("[GameManager] ⚠️ Ibu NPC not assigned!");
        }

        if (bapakNPC != null)
        {
            bapakNPC.SetActive(true);

            // ✅ ENABLE VISION CONE!
            NPCVisionCone bapakVision = bapakNPC.GetComponent<NPCVisionCone>();
            if (bapakVision != null)
            {
                bapakVision.enabled = true;
                Debug.Log("[GameManager] ✅ Bapak vision cone ENABLED!");
            }

            Debug.Log("[GameManager] ✅ Bapak NPC activated!");
        }
        else
        {
            Debug.LogWarning("[GameManager] ⚠️ Bapak NPC not assigned!");
        }

        // ✅ ACTIVATE STEALTH MANAGER!
        if (stealthManager != null)
        {
            // ENSURE GAMEOBJECT IS ACTIVE!
            if (!stealthManager.gameObject.activeSelf)
            {
                stealthManager.gameObject.SetActive(true);
                Debug.Log("[GameManager] ✅ StealthManager GameObject activated!");
            }

            stealthManager.ActivateMission();
            Debug.Log("[GameManager] ✅ StealthManager mission activated - detection is now LIVE!");
        }
        else
        {
            Debug.LogError("[GameManager] ❌ StealthManager not found! Detection will not work!");
        }

        Debug.Log("[GameManager] ✅ Stealth mission setup complete!");
    }

    /// <summary>
    /// Load Main Menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        Debug.Log("[GameManager] Loading Main Menu...");

        // ✅ RESET PROGRESS SEBELUM KE MAIN MENU!
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
            Debug.Log("[GameManager] Progress reset to ACT 1!");
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Reset time scale (kalau ada pause)
        Time.timeScale = 1;

        // Load main menu scene
        SceneManager.LoadScene("Main Menu");
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ========================================
    // PUBLIC GETTERS
    // ========================================

    public int GetPlayerChoice()
    {
        return playerChoice;
    }

    public bool IsHappyEnding()
    {
        return playerChoice == 1;
    }

    public bool IsStealthRoute()
    {
        return playerChoice == 2;
    }
}