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

    [Header("✨ PHONE VFX - VISUAL EFFECTS")]
    public Image flashOverlay; // White flash effect
    public Image vignetteOverlay; // Dark edges effect
    public AudioClip phoneRevealSound; // Dramatic "DUN DUN" sound
    public AudioClip shockSound; // Additional shock sound
    public float flashDuration = 0.3f;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.15f;
    public float popInDuration = 0.6f;
    public float vignetteDuration = 0.5f;
    public bool enableSlowMotion = false;
    public float slowMotionScale = 0.3f;
    public float slowMotionDuration = 1f;

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
    public float badEndingDisplayTime = 2f;

    [Header("⏭️ ACT 3 PROGRESSION (ROUTE 2)")]
    public bool loadAct3AfterLieDialogue = true;
    public string loadingSceneName = "LoadingScene";
    public int act3Number = 3;
    public int act3Day = 60;

    [Header("🌑 BLACK FADE TRANSITION")]
    public Image blackFadeImage;
    public float fadeOutDuration = 0.8f;

    [Header("🧹 CLEANUP QUEST (ROUTE 1)")]
    public int pauseAtLineIndex = 9;
    public TrashCleanupManager cleanupManager;

    private bool choiceShown = false;
    private int currentDialogueLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isHonestRoute = false;
    private bool isPausedForCleanup = false;
    private Vector3 originalCameraPos;

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
            c.a = 0f;
            blackFadeImage.color = c;
            blackFadeImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Act2Choice] ⚠️ Black Fade Image not assigned!");
        }

        // ✅ SETUP VFX OVERLAYS
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(false);
        }

        if (vignetteOverlay != null)
        {
            vignetteOverlay.gameObject.SetActive(false);
        }

        if (cleanupManager == null)
        {
            cleanupManager = FindObjectOfType<TrashCleanupManager>();
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Save original camera position for shake
        if (cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }

        Debug.Log("[Act2Choice] Script initialized with VFX support!");
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

        // ✅ TRIGGER PHONE VFX SAAT PAPA NUNJUKIN BUKTI!
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

        if (isHonestRoute)
        {
            ShowGoodEnding();
        }
        else
        {
            ShowBadEndingCanvas();
        }
    }

    void ShowGoodEnding()
    {
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
        Debug.Log("[Act2Choice] 🎬 BAD ENDING - Starting instant transition!");

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartCoroutine(InstantFadeToAct3());
        }
        else
        {
            StartCoroutine(DirectFadeToAct3());
        }
    }

    IEnumerator InstantFadeToAct3()
    {
        Debug.Log("[Act2Choice] Starting instant fade sequence...");

        yield return new WaitForSeconds(badEndingDisplayTime);

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(false);
        }

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

            Color finalColor = blackFadeImage.color;
            finalColor.a = 1f;
            blackFadeImage.color = finalColor;

            Debug.Log("[Act2Choice] ✅ Screen BLACK - Loading Act 3 NOW!");
        }

        PrepareAndLoadAct3();
    }

    IEnumerator DirectFadeToAct3()
    {
        Debug.Log("[Act2Choice] Direct fade (no panel) - instant!");

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

            Color finalColor = blackFadeImage.color;
            finalColor.a = 1f;
            blackFadeImage.color = finalColor;
        }

        PrepareAndLoadAct3();
    }

    void PrepareAndLoadAct3()
    {
        Debug.Log("[Act2Choice] ⏭️ LOADING ACT 3 IMMEDIATELY!");

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetProgress(act3Number, act3Day);

            PlayerPrefs.SetInt("CurrentAct", act3Number);
            PlayerPrefs.SetInt("CurrentDay", act3Day);
            PlayerPrefs.Save();

            Debug.Log($"[Act2Choice] ✅ SAVED: ACT {act3Number} DAY {act3Day}");
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (loadAct3AfterLieDialogue)
        {
            Debug.Log("[Act2Choice] 🎮 LOADING SCENE NOW!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadingSceneName);
        }
    }

    public void ShowGoodEndingAfterCleanup()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        ShowGoodEnding();
    }

    IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(endingDisplayTime);

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

    // ✅✅✅ ENHANCED PHONE EVIDENCE WITH DRAMATIC VFX! ✅✅✅
    void ShowPhoneEvidence()
    {
        if (phoneEvidencePrefab == null || cameraTransform == null) return;

        Debug.Log("[Act2Choice] 📱 Showing phone evidence with VFX!");

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.TransformDirection(phoneOffset);
        Quaternion spawnRotation = cameraTransform.rotation * Quaternion.Euler(phoneRotation);

        currentPhoneEvidence = Instantiate(phoneEvidencePrefab, spawnPosition, spawnRotation);
        currentPhoneEvidence.transform.SetParent(cameraTransform);
        currentPhoneEvidence.transform.localScale = Vector3.zero; // Start invisible

        // ✅ TRIGGER ALL VFX AT ONCE!
        StartCoroutine(PhoneRevealVFXCombo());
    }

    // ✅ MAIN VFX COMBO - EXECUTE ALL EFFECTS!
    IEnumerator PhoneRevealVFXCombo()
    {
        Debug.Log("[Act2Choice] ⚡ EXECUTING VFX COMBO!");

        // 1️⃣ FLASH EFFECT
        StartCoroutine(FlashEffect());

        // 2️⃣ CAMERA SHAKE
        StartCoroutine(CameraShake());

        // 3️⃣ DRAMATIC SOUND
        if (audioSource != null && phoneRevealSound != null)
        {
            audioSource.PlayOneShot(phoneRevealSound);
        }

        // Optional shock sound
        if (shockSound != null)
        {
            yield return new WaitForSeconds(0.2f);
            audioSource.PlayOneShot(shockSound);
        }

        // 4️⃣ SLOW MOTION (optional)
        if (enableSlowMotion)
        {
            StartCoroutine(SlowMotionEffect());
        }

        // 5️⃣ PHONE POP-IN ANIMATION
        StartCoroutine(PhonePopInAnimation());

        // 6️⃣ VIGNETTE EFFECT (dark edges)
        StartCoroutine(VignetteEffect());

        yield return null;
    }

    // 🎆 FLASH EFFECT - White screen flash
    IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;

        Debug.Log("[VFX] Flash effect!");

        flashOverlay.gameObject.SetActive(true);
        Color c = flashOverlay.color;
        c.a = 0f;
        flashOverlay.color = c;

        // Flash IN (quickly)
        float elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.9f, elapsed / (flashDuration / 2));
            c.a = alpha;
            flashOverlay.color = c;
            yield return null;
        }

        // Flash OUT (quickly)
        elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.9f, 0f, elapsed / (flashDuration / 2));
            c.a = alpha;
            flashOverlay.color = c;
            yield return null;
        }

        flashOverlay.gameObject.SetActive(false);
    }

    // 📷 CAMERA SHAKE - Screen shake effect
    IEnumerator CameraShake()
    {
        if (cameraTransform == null) yield break;

        Debug.Log("[VFX] Camera shake!");

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            cameraTransform.localPosition = originalCameraPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original position
        cameraTransform.localPosition = originalCameraPos;
    }

    // ⏱️ SLOW MOTION EFFECT
    IEnumerator SlowMotionEffect()
    {
        Debug.Log("[VFX] Slow motion!");

        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }

    // 📱 PHONE POP-IN ANIMATION - Elastic bounce
    IEnumerator PhonePopInAnimation()
    {
        if (currentPhoneEvidence == null) yield break;

        Debug.Log("[VFX] Phone pop-in animation!");

        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;

        while (elapsed < popInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popInDuration;

            // Elastic easing for bounce effect
            float scale = ElasticEaseOut(t);
            currentPhoneEvidence.transform.localScale = targetScale * scale;

            yield return null;
        }

        currentPhoneEvidence.transform.localScale = targetScale;
    }

    // 🎨 ELASTIC EASING FUNCTION
    float ElasticEaseOut(float t)
    {
        if (t == 0 || t == 1) return t;

        float p = 0.3f;
        float s = p / 4f;

        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }

    // 🌑 VIGNETTE EFFECT - Dark edges
    IEnumerator VignetteEffect()
    {
        if (vignetteOverlay == null) yield break;

        Debug.Log("[VFX] Vignette effect!");

        vignetteOverlay.gameObject.SetActive(true);
        Color c = vignetteOverlay.color;
        c.a = 0f;
        vignetteOverlay.color = c;

        // Fade in vignette
        float elapsed = 0f;
        while (elapsed < vignetteDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.7f, elapsed / vignetteDuration);
            c.a = alpha;
            vignetteOverlay.color = c;
            yield return null;
        }

        // Keep vignette active until phone is hidden
    }

    void HidePhoneEvidence()
    {
        if (currentPhoneEvidence != null)
        {
            Destroy(currentPhoneEvidence);
            currentPhoneEvidence = null;
        }

        // ✅ ALSO HIDE VIGNETTE
        if (vignetteOverlay != null)
        {
            vignetteOverlay.gameObject.SetActive(false);
        }

        Debug.Log("[Act2Choice] Phone evidence hidden + VFX cleaned up!");
    }
}