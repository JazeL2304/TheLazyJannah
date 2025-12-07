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
        new DialogueLine { speaker = "PAPA", text = "Baik Pa. Mulai sekarang, beresin kamarmu dulu ya." }
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

    [Header("📱 PHONE EVIDENCE (ROUTE 2)")]
    public GameObject phoneEvidencePrefab;
    public Transform cameraTransform;
    public Vector3 phoneOffset = new Vector3(0f, -0.2f, 0.5f);
    public Vector3 phoneRotation = new Vector3(0f, 180f, 0f);
    public float phoneDisplayDuration = 3f;
    public int showPhoneAtLineIndex = 1;
    private GameObject currentPhoneEvidence;

    [Header("🎬 GOOD ENDING PANEL (ROUTE 1 ONLY)")]
    public GameObject goodEndingPanel;

    [Header("🎵 GOOD ENDING MUSIC (Optional)")]
    public AudioClip goodEndingMusic;

    [Header("🎮 ENDING SCENE MANAGEMENT")]
    public bool loadMainMenuAfterEnding = true;
    public float endingDisplayTime = 8f;
    public string mainMenuSceneName = "Main Menu";

    [Header("🎬 BAD ENDING PANEL (ROUTE 2)")]
    public GameObject badEndingPanel;
    public float badEndingDisplayTime = 5f;

    [Header("⏭️ ACT 3 PROGRESSION (ROUTE 2)")]
    public bool loadAct3AfterLieDialogue = true;
    public float delayBeforeAct3 = 2f;
    public string loadingSceneName = "LoadingScene";
    public int act3Number = 3;
    public int act3Day = 60;

    [Header("🌑 BLACK FADE TRANSITION")]
    public Image blackFadeImage; // Drag Image hitam fullscreen dari Canvas
    public float fadeOutDuration = 1.5f; // Durasi fade ke hitam

    [Header("🧹 CLEANUP QUEST (ROUTE 1)")]
    public int pauseAtLineIndex = 9;
    public TrashCleanupManager cleanupManager;

    private bool choiceShown = false;
    private int currentDialogueLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isHonestRoute = false;
    private bool isPausedForCleanup = false;

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

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(false);
        }

        // Setup black fade image
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 0f; // Start transparent
            blackFadeImage.color = c;
            blackFadeImage.gameObject.SetActive(true); // Keep active but transparent
        }

        if (cleanupManager == null)
        {
            cleanupManager = FindObjectOfType<TrashCleanupManager>();
            if (cleanupManager != null)
            {
                Debug.Log("[Act2Choice] TrashCleanupManager auto-detected!");
            }
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform != null)
            {
                Debug.Log("[Act2Choice] Camera auto-detected!");
            }
        }

        Debug.Log("[Act2Choice] Script initialized!");
    }

    void Update()
    {
        if (isPausedForCleanup)
        {
            if (cleanupManager != null && cleanupManager.IsQuestComplete())
            {
                // Waiting for parent interaction
            }
            return;
        }

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

        Invoke("StartDialogueSequence", 0.2f);
    }

    void OnChoice2Selected()
    {
        Debug.Log("[Act2Choice] ===== ROUTE 2: BOHONG → LANJUT ACT 3 =====");

        PlayClickSound();
        HideChoicePanel();

        isHonestRoute = false;

        Invoke("StartDialogueSequence", 0.2f);
    }

    void StartDialogueSequence()
    {
        Debug.Log("[Act2Choice] 🎭 StartDialogueSequence() CALLED!");

        currentDialogueLine = 0;
        dialogueActive = true;
        isPausedForCleanup = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(TypeDialogueLine());
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

        if (!isHonestRoute && currentDialogueLine == showPhoneAtLineIndex)
        {
            ShowPhoneEvidence();
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
        if (currentPhoneEvidence != null)
        {
            HidePhoneEvidence();
        }

        if (isHonestRoute && currentDialogueLine == pauseAtLineIndex)
        {
            Debug.Log("[Act2Choice] 🛑 PAUSE untuk beresin kamar!");

            isPausedForCleanup = true;
            dialogueActive = false;

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (cleanupManager != null)
            {
                cleanupManager.StartCleanupQuest();
            }

            return;
        }

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
            Debug.LogWarning("[Act2Choice] Honest route finished without cleanup quest!");
            ShowGoodEnding();
        }
        else
        {
            // ROUTE 2: Tampilkan Bad Ending Canvas dulu sebelum Act 3
            ShowBadEndingCanvas();
        }
    }

    void ShowGoodEnding()
    {
        Debug.Log("[Act2Choice] 🎉 SHOWING GOOD ENDING - GAME SELESAI!");

        if (goodEndingPanel != null)
        {
            goodEndingPanel.SetActive(true);

            if (goodEndingMusic != null && audioSource != null)
            {
                audioSource.clip = goodEndingMusic;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (loadMainMenuAfterEnding)
            {
                StartCoroutine(LoadMainMenuAfterDelay());
            }
        }
    }

    void ShowBadEndingCanvas()
    {
        Debug.Log("[Act2Choice] 🎬 SHOWING BAD ENDING CANVAS - ACT 2 END!");

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(true);
            Debug.Log("[Act2Choice] ✅ Bad Ending Canvas displayed!");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Setelah beberapa detik, lanjut ke Act 3 dengan fade
            StartCoroutine(WaitThenPrepareAct3());
        }
        else
        {
            Debug.LogWarning("[Act2Choice] ⚠️ Bad Ending Panel not assigned! Langsung ke Act 3...");
            PrepareAct3Transition();
        }
    }

    IEnumerator WaitThenPrepareAct3()
    {
        Debug.Log($"[Act2Choice] Waiting {badEndingDisplayTime} seconds before Act 3...");
        yield return new WaitForSeconds(badEndingDisplayTime);

        // FADE OUT BAD ENDING PANEL
        if (badEndingPanel != null)
        {
            CanvasGroup canvasGroup = badEndingPanel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = badEndingPanel.AddComponent<CanvasGroup>();
            }

            float fadeTime = 1f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                yield return null;
            }

            badEndingPanel.SetActive(false);
        }

        // FADE TO BLACK SCREEN
        if (blackFadeImage != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);

                Color c = blackFadeImage.color;
                c.a = alpha;
                blackFadeImage.color = c;

                yield return null;
            }

            // Ensure fully black
            Color finalColor = blackFadeImage.color;
            finalColor.a = 1f;
            blackFadeImage.color = finalColor;
        }

        // Delay tambahan di black screen
        yield return new WaitForSeconds(0.5f);

        // Lanjut ke Act 3
        PrepareAct3Transition();
    }

    public void ShowGoodEndingAfterCleanup()
    {
        Debug.Log("[Act2Choice] 🎉 Cleanup quest selesai! Showing Good Ending...");

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        ShowGoodEnding();
    }

    void PrepareAct3Transition()
    {
        Debug.Log("[Act2Choice] ⏭️ PREPARING ACT 3 TRANSITION...");

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetProgress(act3Number, act3Day);
            Debug.Log($"[Act2Choice] ✅ Progress set to ACT {act3Number} DAY {act3Day}");
        }

        if (loadAct3AfterLieDialogue)
        {
            StartCoroutine(LoadAct3AfterDelay());
        }
    }

    IEnumerator LoadAct3AfterDelay()
    {
        Debug.Log($"[Act2Choice] Loading Act 3 in {delayBeforeAct3} seconds...");

        yield return new WaitForSeconds(delayBeforeAct3);

        Debug.Log("[Act2Choice] 🎮 Loading Act 3 via Loading Scene...");

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

    public void ManualLoadAct3()
    {
        Debug.Log("[Act2Choice] Manual Act 3 load triggered!");
        PrepareAct3Transition();
    }

    void ShowPhoneEvidence()
    {
        if (phoneEvidencePrefab == null)
        {
            Debug.LogWarning("[Act2Choice] Phone Evidence Prefab not assigned!");
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("[Act2Choice] Camera Transform not found!");
            return;
        }

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.TransformDirection(phoneOffset);
        Quaternion spawnRotation = cameraTransform.rotation * Quaternion.Euler(phoneRotation);

        currentPhoneEvidence = Instantiate(phoneEvidencePrefab, spawnPosition, spawnRotation);
        currentPhoneEvidence.transform.SetParent(cameraTransform);

        Debug.Log("[Act2Choice] 📱 Phone evidence displayed!");
    }

    void HidePhoneEvidence()
    {
        if (currentPhoneEvidence != null)
        {
            Destroy(currentPhoneEvidence);
            currentPhoneEvidence = null;
            Debug.Log("[Act2Choice] 📱 Phone evidence hidden!");
        }
    }
}