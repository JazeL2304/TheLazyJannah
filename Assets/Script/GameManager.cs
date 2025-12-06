using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
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

    [Header("Post-Choice 2 Settings")]
    public bool showDialogueAfterChoice2 = true;
    public float delayBeforeDialogue = 1f;

    private int playerChoice = 0;
    private bool waitingForDialogueComplete = false;

    void Start()
    {
        // Cek duplicate GameManager
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Hide semua panel di awal
        if (happyEndingPanel != null) happyEndingPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (stealthQuestUI != null) stealthQuestUI.SetActive(false);

        // Hide NPCs di awal
        if (ibuNPC != null) ibuNPC.SetActive(false);
        if (bapakNPC != null) bapakNPC.SetActive(false);

        // Auto-detect QuestManager jika tidak di-assign
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        Debug.Log("[GameManager] GameManager initialized!");
    }

    public void OnPlayerChoice(int choice)
    {
        playerChoice = choice;
        Debug.Log("=== PLAYER CHOICE: " + choice + " ===");

        if (choice == 1)
        {
            Debug.Log("[GameManager] Player memilih: JANGAN CURI");
            ShowHappyEnding();
        }
        else if (choice == 2)
        {
            Debug.Log("[GameManager] Player memilih: AMBIL KARTU KREDIT");
            StartCoroutine(StartStealthMissionSequence());
        }
    }

    void ShowHappyEnding()
    {
        Debug.Log("[GameManager] >>> ENDING 1/3: HAPPY ENDING <<<");

        // Matikan semua panel lain
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("[GameManager] ChoicePanel dinonaktifkan");
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
            Debug.Log("[GameManager] DialogueBox dinonaktifkan");
        }

        // Matikan DialogueManager script
        if (dialogueManager != null)
        {
            dialogueManager.gameObject.SetActive(false);
            Debug.Log("[GameManager] DialogueManager dinonaktifkan");
        }

        // Reset EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            eventSystem.enabled = false;
            eventSystem.enabled = true;
        }

        // Tampilkan Happy Ending Panel
        if (happyEndingPanel != null)
        {
            happyEndingPanel.SetActive(true);
            Debug.Log("[GameManager] HappyEndingPanel ditampilkan");
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tampilkan main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
        }

        Debug.Log("[GameManager] Game selesai dengan Happy Ending!");
    }

    IEnumerator StartStealthMissionSequence()
    {
        Debug.Log("[GameManager] >>> Memulai sequence stealth mission...");

        // Matikan ChoicePanel
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("[GameManager] ChoicePanel dinonaktifkan");
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // Tampilkan dialog lanjutan
        if (showDialogueAfterChoice2 && dialogueManager != null)
        {
            waitingForDialogueComplete = true;
            dialogueManager.ContinueDialogueAfterChoice2();
            Debug.Log("[GameManager] Dialog lanjutan ditampilkan - Menunggu player klik...");

            // Tunggu sampai dialog selesai
            while (waitingForDialogueComplete)
            {
                yield return null;
            }

            Debug.Log("[GameManager] Dialog selesai - Melanjutkan ke stealth mission");
        }

        // Mulai misi stealth
        StartStealthMission();
    }

    public void OnDialogueCompleteAfterChoice2()
    {
        Debug.Log("[GameManager] Dialog post-choice 2 selesai");
        waitingForDialogueComplete = false;
    }

    void StartStealthMission()
    {
        Debug.Log("[GameManager] === MISI STEALTH DIMULAI ===");

        // Lock cursor untuk gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Start quest via QuestManager
        if (questManager != null)
        {
            questManager.StartQuest(0);
            Debug.Log("[GameManager] Quest dimulai via QuestManager: MISI STEALTH");
        }
        else
        {
            // Fallback: Tampilkan UI manual
            if (stealthQuestUI != null)
            {
                stealthQuestUI.SetActive(true);
                Debug.Log("[GameManager] Quest UI ditampilkan (fallback manual)");
            }
        }

        // Aktifkan NPCs
        if (ibuNPC != null)
        {
            ibuNPC.SetActive(true);
            Debug.Log("[GameManager] NPC Ibu diaktifkan");
        }

        if (bapakNPC != null)
        {
            bapakNPC.SetActive(true);
            Debug.Log("[GameManager] NPC Bapak diaktifkan");
        }

        Debug.Log("[GameManager] Tekan F untuk berdiri dan mulai misi!");
    }

    public void EndGame1()
    {
        Debug.Log("[GameManager] Loading Phone scene...");
        SceneManager.LoadScene("Phone");
    }

    public void LoadMainMenu()
    {
        Debug.Log("[GameManager] Loading Main Menu...");

        // ✅ JANGAN RESET PROGRESS - Progress harus persistent!
        if (GameProgressManager.Instance != null)
        {
            int act = GameProgressManager.Instance.currentAct;
            int day = GameProgressManager.Instance.currentDay;
            Debug.Log($"[GameManager] Loading Main Menu with progress: ACT {act} DAY {day}");
        }

        // Reset timescale
        Time.timeScale = 1;

        // Load main menu
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Getter functions
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