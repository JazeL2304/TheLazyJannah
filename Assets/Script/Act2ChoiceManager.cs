using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Act2ChoiceManager : MonoBehaviour
{
    [Header("Choice UI Elements")]
    public GameObject choicePanel;
    public Button choice1Button;
    public Button choice2Button;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;

    [Header("Choice Settings")]
    public string choice1Label = "Mengaku dan minta maaf";
    public string choice2Label = "Diam dan pura-pura tidak tahu";

    [Header("Audio")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    [Header("🎭 DIALOGUE SYSTEM")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker = "JANNAH";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("📝 ROUTE 1: JUJUR → GOOD ENDING")]
    public DialogueLine[] honestDialogues = new DialogueLine[]
    {
        new DialogueLine { speaker = "JANNAH", text = "Ma... Pa... Maafin aku..." },
        new DialogueLine { speaker = "PAPA", text = "Jannah? Ada apa?" },
        new DialogueLine { speaker = "JANNAH", text = "Aku... aku yang ambil kartu kredit kalian. Aku pakai buat beli skin game." },
        new DialogueLine { speaker = "MAMA", text = "Jannah... kenapa kamu lakukan itu? Kenapa tidak bilang ke Mama kalau butuh sesuatu?" },
        new DialogueLine { speaker = "JANNAH", text = "Aku takut... aku tahu ini salah. Aku malu sama diri sendiri." },
        new DialogueLine { speaker = "PAPA", text = "Setidaknya kamu jujur sekarang. Papa senang kamu berani mengakuinya." },
        new DialogueLine { speaker = "JANNAH", text = "Aku janji akan berubah. Mulai dari sekarang, aku akan jadi anak yang lebih baik." },
        new DialogueLine { speaker = "JANNAH", text = "Aku akan rajin belajar, beresin kamar, dan nggak akan bohong lagi." },
        new DialogueLine { speaker = "MAMA", text = "Mama bangga sama kamu, Jannah. Mengakui kesalahan itu butuh keberanian." },
        new DialogueLine { speaker = "PAPA", text = "Ayo kita mulai dari awal lagi. Papa dan Mama akan bantu kamu." }
    };

    [Header("📝 ROUTE 2: BOHONG → LANJUT ACT 3")]
    public DialogueLine[] lieDialogues = new DialogueLine[]
    {
        new DialogueLine { speaker = "JANNAH", text = "Aku nggak tau apa-apa! Kenapa kalian tuduh aku?" },
        new DialogueLine { speaker = "PAPA", text = "Jannah, kita sudah dapat buktinya. Ini tagihan dari kartu kredit Papa." },
        new DialogueLine { speaker = "PAPA", text = "Semuanya transaksi game. Dan itu terjadi pas kamu di rumah sendirian." },
        new DialogueLine { speaker = "JANNAH", text = "Itu bukan aku! Mungkin orang lain yang pake!" },
        new DialogueLine { speaker = "MAMA", text = "Jannah, tolong jujur sama Mama. Kita bisa selesaikan ini baik-baik kalau kamu jujur." },
        new DialogueLine { speaker = "JANNAH", text = "MAMA NGGAK PERCAYA AKU?! KALIAN BERDUA SELALU NUDUH AKU!" },
        new DialogueLine { speaker = "PAPA", text = "Jannah! Jangan bicara seperti itu sama Mama kamu!" },
        new DialogueLine { speaker = "JANNAH", text = "DIAM! KELUAR DARI KAMARKU! AKU BENCI KALIAN!" },
        new DialogueLine { speaker = "JANNAH", text = "PERGI! JANGAN GANGGU AKU LAGI!" },
        new DialogueLine { speaker = "MAMA", text = "Jannah... kenapa kamu..." },
        new DialogueLine { speaker = "PAPA", text = "Sudah Ma... Biarkan dia sendiri dulu. Ayo kita keluar." }
    };

    [Header("🎬 GOOD ENDING PANEL (ROUTE 1 ONLY)")]
    public GameObject goodEndingPanel;  // Panel sudah lengkap dengan Image & Text di dalamnya

    [Header("🎵 GOOD ENDING MUSIC (Optional)")]
    public AudioClip goodEndingMusic;

    [Header("🎮 ENDING SCENE MANAGEMENT")]
    public bool loadMainMenuAfterEnding = true;
    public float endingDisplayTime = 8f;
    public string mainMenuSceneName = "Main Menu";

    [Header("⏭️ ACT 3 PROGRESSION (ROUTE 2)")]
    public bool loadAct3AfterLieDialogue = true;
    public float delayBeforeAct3 = 2f;
    public string loadingSceneName = "LoadingScene";
    public int act3Number = 3;
    public int act3Day = 60; // Misal 30 hari setelah Act 2

    private bool choiceShown = false;
    private int currentDialogueLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isHonestRoute = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (choice1Text != null) choice1Text.text = choice1Label;
        if (choice2Text != null) choice2Text.text = choice2Label;

        if (choice1Button != null)
        {
            choice1Button.onClick.AddListener(OnChoice1Selected);
        }
        if (choice2Button != null)
        {
            choice2Button.onClick.AddListener(OnChoice2Selected);
        }

        HideChoicePanel();

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (goodEndingPanel != null)
        {
            goodEndingPanel.SetActive(false);
        }

        Debug.Log("[Act2Choice] Script initialized!");
    }

    void Update()
    {
        if (dialogueActive && Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

            if (isTyping)
            {
                StopAllCoroutines();

                DialogueLine[] currentDialogues = isHonestRoute ? honestDialogues : lieDialogues;
                dialogueText.text = currentDialogues[currentDialogueLine].text;

                isTyping = false;
            }
            else
            {
                NextDialogueLine();
            }
        }
    }

    public void ShowChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
            choiceShown = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("[Act2Choice] Choice Panel ditampilkan!");
        }
        else
        {
            Debug.LogError("[Act2Choice] Choice Panel NULL! Set di Inspector!");
        }
    }

    public void HideChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            choiceShown = false;
        }
    }

    void OnChoice1Selected()
    {
        Debug.Log("[Act2Choice] ===== ROUTE 1: JUJUR → GOOD ENDING =====");

        PlayClickSound();
        HideChoicePanel();

        isHonestRoute = true;

        // Delay sedikit sebelum start dialogue (untuk ensure panel sudah hide)
        Invoke("StartDialogueSequence", 0.2f);
    }

    void OnChoice2Selected()
    {
        Debug.Log("[Act2Choice] ===== ROUTE 2: BOHONG → LANJUT ACT 3 =====");

        PlayClickSound();
        HideChoicePanel();

        isHonestRoute = false;

        // Delay sedikit sebelum start dialogue (untuk ensure panel sudah hide)
        Invoke("StartDialogueSequence", 0.2f);
    }

    void StartDialogueSequence()
    {
        Debug.Log("[Act2Choice] 🎭 StartDialogueSequence() CALLED!");
        Debug.Log($"[Act2Choice] This GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"[Act2Choice] This GameObject name: {gameObject.name}");

        currentDialogueLine = 0;
        dialogueActive = true;

        if (dialogueBox != null)
        {
            Debug.Log($"[Act2Choice] DialogueBox found: {dialogueBox.name}");
            dialogueBox.SetActive(true);
            Debug.Log("[Act2Choice] DialogueBox activated!");
        }
        else
        {
            Debug.LogError("[Act2Choice] ❌ DialogueBox is NULL!");
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[Act2Choice] Starting {(isHonestRoute ? "HONEST" : "LIE")} dialogue sequence...");

        // START COROUTINE DENGAN SAFETY CHECK
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(TypeDialogueLine());
            Debug.Log("[Act2Choice] ✅ Coroutine started successfully!");
        }
        else
        {
            Debug.LogError("[Act2Choice] ❌ Cannot start coroutine - GameObject is inactive!");
        }
    }

    IEnumerator TypeDialogueLine()
    {
        isTyping = true;
        dialogueText.text = "";

        DialogueLine[] currentDialogues = isHonestRoute ? honestDialogues : lieDialogues;
        DialogueLine currentLine = currentDialogues[currentDialogueLine];

        if (dialogueNameText != null)
        {
            dialogueNameText.text = currentLine.speaker;
        }

        foreach (char c in currentLine.text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextDialogueLine()
    {
        currentDialogueLine++;

        DialogueLine[] currentDialogues = isHonestRoute ? honestDialogues : lieDialogues;

        if (currentDialogueLine < currentDialogues.Length)
        {
            StartCoroutine(TypeDialogueLine());
        }
        else
        {
            EndDialogueSequence();
        }
    }

    void EndDialogueSequence()
    {
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[Act2Choice] Dialogue sequence finished!");

        if (isHonestRoute)
        {
            // ROUTE 1: GOOD ENDING
            ShowGoodEnding();
        }
        else
        {
            // ROUTE 2: LANJUT KE ACT 3
            PrepareAct3Transition();
        }
    }

    void ShowGoodEnding()
    {
        Debug.Log("[Act2Choice] 🎉 SHOWING GOOD ENDING - GAME SELESAI!");

        if (goodEndingPanel != null)
        {
            goodEndingPanel.SetActive(true);
            Debug.Log("[Act2Choice] ✅ Good Ending Panel displayed!");

            // Play music jika ada
            if (goodEndingMusic != null && audioSource != null)
            {
                audioSource.clip = goodEndingMusic;
                audioSource.loop = true;
                audioSource.Play();
                Debug.Log("[Act2Choice] 🎵 Good Ending Music playing!");
            }

            if (loadMainMenuAfterEnding)
            {
                StartCoroutine(LoadMainMenuAfterDelay());
            }
        }
        else
        {
            Debug.LogError("[Act2Choice] ❌ Good Ending Panel not assigned!");
        }
    }

    void PrepareAct3Transition()
    {
        Debug.Log("[Act2Choice] ⏭️ PREPARING ACT 3 TRANSITION...");

        // Set progress ke Act 3
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetProgress(act3Number, act3Day);
            Debug.Log($"[Act2Choice] ✅ Progress set to ACT {act3Number} DAY {act3Day}");
        }
        else
        {
            Debug.LogWarning("[Act2Choice] ⚠️ GameProgressManager not found!");
        }

        if (loadAct3AfterLieDialogue)
        {
            StartCoroutine(LoadAct3AfterDelay());
        }
        else
        {
            Debug.Log("[Act2Choice] Auto-load Act 3 disabled. Waiting for manual trigger.");
        }
    }

    IEnumerator LoadAct3AfterDelay()
    {
        Debug.Log($"[Act2Choice] Loading Act 3 in {delayBeforeAct3} seconds...");

        yield return new WaitForSeconds(delayBeforeAct3);

        Debug.Log("[Act2Choice] 🎮 Loading Act 3 via Loading Scene...");

        // Stop music
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(loadingSceneName);
    }

    IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(endingDisplayTime);

        Debug.Log("[Act2Choice] Loading Main Menu...");

        // Stop music
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public bool IsChoiceShown()
    {
        return choiceShown;
    }

    // PUBLIC FUNCTION - Untuk dipanggil manual jika perlu
    public void ManualLoadAct3()
    {
        Debug.Log("[Act2Choice] Manual Act 3 load triggered!");
        PrepareAct3Transition();
    }
}