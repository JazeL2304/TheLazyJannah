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

    [Header("📝 DIALOGUE - CHOICE 1 (JUJUR)")]
    [TextArea(3, 10)]
    public string[] honestDialogues = new string[]
    {
        "Maaf, Mama... Papa... Aku yang ambil kartu kredit kalian.",
        "Aku... aku butuh uang untuk beli skin game. Aku tahu ini salah.",
        "Aku janji akan jadi anak yang lebih baik. Mulai dari membereskan kamar ini.",
        "Aku akan lebih rajin belajar dan tidak akan mencuri lagi."
    };

    [Header("📝 DIALOGUE - CHOICE 2 (BOHONG)")]
    public DialogueLine[] lieDialogues = new DialogueLine[]
    {
        new DialogueLine { speaker = "PAPA", text = "Jannah, kita sudah dapat bukti tagihan dari HP kita. Ini kartu kredit kita yang kamu pakai!" },
        new DialogueLine { speaker = "JANNAH", text = "Itu bukan aku! Aku nggak tahu apa-apa!" },
        new DialogueLine { speaker = "MAMA", text = "Jannah, jangan berbohong! Kita sudah lihat history transaksinya. Ini untuk game, kan?" },
        new DialogueLine { speaker = "JANNAH", text = "KALIAN NGGAK PERCAYA AKU! KELUAR DARI KAMARKU!" },
        new DialogueLine { speaker = "JANNAH", text = "AKU BENCI KALIAN! PERGI!" }
    };

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker = "JANNAH";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("🎬 ENDING PANELS")]
    public GameObject goodEndingPanel;  // Panel untuk Good Ending (pelukan bahagia)
    public GameObject badEndingPanel;   // Panel untuk Bad Ending (hati retak)

    [Header("🖼️ ENDING IMAGES")]
    public Image goodEndingImage;  // Image component untuk gambar pelukan
    public Image badEndingImage;   // Image component untuk gambar hati retak
    public Sprite happyFamilySprite;  // Sprite pelukan bahagia
    public Sprite brokenHeartSprite;  // Sprite hati retak

    [Header("🎮 SCENE MANAGEMENT")]
    public bool loadMainMenuAfterEnding = true;
    public float endingDisplayTime = 5f;  // Durasi tampilan ending sebelum ke menu
    public string mainMenuSceneName = "Main Menu";

    private bool choiceShown = false;
    private int currentDialogueLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isHonestRoute = false;

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set text labels
        if (choice1Text != null) choice1Text.text = choice1Label;
        if (choice2Text != null) choice2Text.text = choice2Label;

        // Add button listeners
        if (choice1Button != null)
        {
            choice1Button.onClick.AddListener(OnChoice1Selected);
        }
        if (choice2Button != null)
        {
            choice2Button.onClick.AddListener(OnChoice2Selected);
        }

        // Hide panels at start
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

        Debug.Log("[Act2Choice] Script initialized!");
    }

    void Update()
    {
        // Handle dialogue progression
        if (dialogueActive && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip typing
                StopAllCoroutines();

                if (isHonestRoute)
                {
                    dialogueText.text = honestDialogues[currentDialogueLine];
                }
                else
                {
                    dialogueText.text = lieDialogues[currentDialogueLine].text;
                }

                isTyping = false;
            }
            else
            {
                // Next line
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

            // Unlock cursor untuk klik button
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

            Debug.Log("[Act2Choice] Choice Panel disembunyikan!");
        }
    }

    void OnChoice1Selected()
    {
        Debug.Log("[Act2Choice] ===== PILIHAN 1: MENGAKU (JUJUR) =====");

        PlayClickSound();
        HideChoicePanel();

        isHonestRoute = true;
        StartDialogueSequence();
    }

    void OnChoice2Selected()
    {
        Debug.Log("[Act2Choice] ===== PILIHAN 2: BOHONG =====");

        PlayClickSound();
        HideChoicePanel();

        isHonestRoute = false;
        StartDialogueSequence();
    }

    void StartDialogueSequence()
    {
        currentDialogueLine = 0;
        dialogueActive = true;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        StartCoroutine(TypeDialogueLine());
    }

    IEnumerator TypeDialogueLine()
    {
        isTyping = true;
        dialogueText.text = "";

        string textToType;
        string speakerName;

        if (isHonestRoute)
        {
            textToType = honestDialogues[currentDialogueLine];
            speakerName = "JANNAH";
        }
        else
        {
            textToType = lieDialogues[currentDialogueLine].text;
            speakerName = lieDialogues[currentDialogueLine].speaker;
        }

        if (dialogueNameText != null)
        {
            dialogueNameText.text = speakerName;
        }

        foreach (char c in textToType.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextDialogueLine()
    {
        currentDialogueLine++;

        int maxLines = isHonestRoute ? honestDialogues.Length : lieDialogues.Length;

        if (currentDialogueLine < maxLines)
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

        // Show appropriate ending
        if (isHonestRoute)
        {
            ShowGoodEnding();
        }
        else
        {
            ShowBadEnding();
        }
    }

    void ShowGoodEnding()
    {
        Debug.Log("[Act2Choice] 🎉 SHOWING GOOD ENDING - Pelukan Bahagia!");

        if (goodEndingPanel != null)
        {
            goodEndingPanel.SetActive(true);

            // Set image jika ada
            if (goodEndingImage != null && happyFamilySprite != null)
            {
                goodEndingImage.sprite = happyFamilySprite;
            }

            Debug.Log("[Act2Choice] ✅ Good Ending Panel displayed!");

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

    void ShowBadEnding()
    {
        Debug.Log("[Act2Choice] 💔 SHOWING BAD ENDING - Hati Retak!");

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(true);

            // Set image jika ada
            if (badEndingImage != null && brokenHeartSprite != null)
            {
                badEndingImage.sprite = brokenHeartSprite;
            }

            Debug.Log("[Act2Choice] ✅ Bad Ending Panel displayed!");

            if (loadMainMenuAfterEnding)
            {
                StartCoroutine(LoadMainMenuAfterDelay());
            }
        }
        else
        {
            Debug.LogError("[Act2Choice] ❌ Bad Ending Panel not assigned!");
        }
    }

    IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(endingDisplayTime);

        Debug.Log("[Act2Choice] Loading Main Menu...");
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
}