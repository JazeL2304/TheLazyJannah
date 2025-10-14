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
    public QuestManager questManager;  // TAMBAHAN BARU

    [Header("Post-Choice 2 Settings")]
    public bool showDialogueAfterChoice2 = true;
    public float delayBeforeDialogue = 1f;

    private int playerChoice = 0;
    private bool waitingForDialogueComplete = false;

    void Start()
    {
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        if (happyEndingPanel != null) happyEndingPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (stealthQuestUI != null) stealthQuestUI.SetActive(false);

        if (ibuNPC != null) ibuNPC.SetActive(false);
        if (bapakNPC != null) bapakNPC.SetActive(false);

        // AUTO-DETECT QUESTMANAGER - TAMBAHAN BARU
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        Debug.Log("GameManager initialized!");
    }

    public void OnPlayerChoice(int choice)
    {
        playerChoice = choice;
        Debug.Log("=== PLAYER CHOICE: " + choice + " ===");

        if (choice == 1)
        {
            Debug.Log("Player memilih: JANGAN CURI");
            ShowHappyEnding();
        }
        else if (choice == 2)
        {
            Debug.Log("Player memilih: AMBIL KARTU KREDIT");
            StartCoroutine(StartStealthMissionSequence());
        }
    }

    void ShowHappyEnding()
    {
        Debug.Log(">>> ENDING 1/3: HAPPY ENDING <<<");

        // MATIKAN SEMUA PANEL LAIN DENGAN PAKSA
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("ChoicePanel dinonaktifkan via SetActive(false)");
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
            Debug.Log("DialogueBox dinonaktifkan");
        }

        // Matikan DialogueManager script juga
        if (dialogueManager != null)
        {
            dialogueManager.gameObject.SetActive(false);
            Debug.Log("DialogueManager dinonaktifkan");
        }

        // Disable EventSystem sementara lalu enable lagi untuk reset
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            eventSystem.enabled = false;
            eventSystem.enabled = true;
        }

        if (happyEndingPanel != null)
        {
            happyEndingPanel.SetActive(true);
            Debug.Log("HappyEndingPanel ditampilkan");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
        }

        Debug.Log("Game selesai dengan Happy Ending!");
    }

    IEnumerator StartStealthMissionSequence()
    {
        Debug.Log(">>> Memulai sequence stealth mission...");

        // Matikan ChoicePanel
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("ChoicePanel dinonaktifkan");
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // Tampilkan dialog lanjutan
        if (showDialogueAfterChoice2 && dialogueManager != null)
        {
            waitingForDialogueComplete = true;
            dialogueManager.ContinueDialogueAfterChoice2();
            Debug.Log("Dialog lanjutan ditampilkan - Menunggu player klik sampai selesai...");

            // Tunggu sampai dialog selesai (callback dari Dialogue.cs)
            while (waitingForDialogueComplete)
            {
                yield return null;
            }

            Debug.Log("Dialog selesai - Melanjutkan ke stealth mission");
        }

        // Mulai misi stealth SETELAH dialog selesai
        StartStealthMission();
    }

    // Fungsi dipanggil dari Dialogue.cs saat dialog post-choice 2 selesai
    public void OnDialogueCompleteAfterChoice2()
    {
        Debug.Log("GameManager menerima notifikasi: Dialog post-choice 2 selesai");
        waitingForDialogueComplete = false;
    }

    void StartStealthMission()
    {
        Debug.Log("=== MISI STEALTH DIMULAI ===");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // START QUEST VIA QUESTMANAGER - TAMBAHAN BARU
        if (questManager != null)
        {
            questManager.StartQuest(0);  // Start quest index 0 (MISI STEALTH)
            Debug.Log("Quest dimulai via QuestManager: MISI STEALTH");
        }
        else
        {
            // FALLBACK: Tampilkan UI manual jika QuestManager tidak ada
            if (stealthQuestUI != null)
            {
                stealthQuestUI.SetActive(true);
                Debug.Log("Quest UI ditampilkan (fallback manual)");
            }
        }

        if (ibuNPC != null)
        {
            ibuNPC.SetActive(true);
            Debug.Log("NPC Ibu diaktifkan");
        }

        if (bapakNPC != null)
        {
            bapakNPC.SetActive(true);
            Debug.Log("NPC Bapak diaktifkan");
        }

        Debug.Log("Tekan F untuk berdiri dan mulai misi!");
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

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
    