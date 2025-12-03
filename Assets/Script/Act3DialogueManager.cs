using UnityEngine;
using TMPro;
using System.Collections;

public class Act3DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("⚙️ Trigger Settings")]
    public float startDelay = 1f; // Delay sebelum dialog mulai
    public bool autoStart = true; // Auto start saat scene load

    [Header("🔊 Audio Settings")]
    public AudioClip clickSound;
    public AudioClip doorKnockSound; // ← BARU - SFX pintu ketok!
    private AudioSource audioSource;

    [System.Serializable]
    public class DialogueLine
    {
        public string characterName = "JANNAH";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("📝 ACT 3 OPENING DIALOGUE")]
    public DialogueLine[] act3Dialogues = new DialogueLine[]
    {
        new DialogueLine {
            characterName = "JANNAH",
            text = "Hah... sudah 60 hari sejak kejadian itu..."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Hubunganku dengan Mama dan Papa semakin memburuk."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Mereka hampir tidak pernah bicara denganku lagi."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Rasanya kamarku jadi semakin gelap dan dingin..."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Aku merasa sangat kesepian."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Mungkin... ini akibat dari pilihanku waktu itu."
        }
    };

    [Header("🚪 DOOR KNOCK CONTINUATION DIALOGUE")]
    public DialogueLine[] doorKnockDialogues = new DialogueLine[]
    {
        new DialogueLine {
            characterName = "JANNAH",
            text = "Hah? Ada yang ketok pintu?"
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Mama? Papa? Kalian pulang?"
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "..."
        },
        new DialogueLine {
            characterName = "JANNAH",
            text = "Atau... itu bukan mereka?"
        }
    };

    [Header("⏳ DOOR KNOCK TIMING")]
    public float delayBeforeDoorKnock = 1f; // Delay sebelum pintu ketok
    public float delayAfterDoorKnock = 1.5f; // Delay setelah sound selesai

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isFirstDialogueComplete = false; // Track dialog pertama selesai

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Hide dialogue box at start
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Check if Act 3
        if (GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;

            if (currentAct == 3)
            {
                if (autoStart)
                {
                    StartCoroutine(AutoStartDialogue());
                }
            }
            else
            {
                Debug.Log("[Act3Dialogue] Not Act 3 - Script disabled");
                this.enabled = false;
            }
        }
    }

    IEnumerator AutoStartDialogue()
    {
        yield return new WaitForSeconds(startDelay);
        StartDialogue();
    }

    void Update()
    {
        if (!dialogueActive) return;

        // LEFT CLICK untuk next/skip
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

            if (isTyping)
            {
                // Skip typing animation
                StopAllCoroutines();

                // Cek apakah masih dialog pertama atau kedua
                DialogueLine[] currentDialogues = isFirstDialogueComplete ? doorKnockDialogues : act3Dialogues;
                dialogueText.text = currentDialogues[currentLine].text;

                isTyping = false;
            }
            else
            {
                // Next line
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        dialogueActive = true;
        currentLine = 0;
        isFirstDialogueComplete = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // Unlock cursor untuk bisa klik
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[Act3Dialogue] Dialogue started!");
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        // Pilih array dialog yang sesuai
        DialogueLine[] currentDialogues = isFirstDialogueComplete ? doorKnockDialogues : act3Dialogues;
        DialogueLine line = currentDialogues[currentLine];

        if (nameText != null)
        {
            nameText.text = line.characterName;
        }

        foreach (char c in line.text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLine++;

        // Pilih array dialog yang sesuai
        DialogueLine[] currentDialogues = isFirstDialogueComplete ? doorKnockDialogues : act3Dialogues;

        if (currentLine < currentDialogues.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            // Cek apakah ini dialog pertama atau kedua yang selesai
            if (!isFirstDialogueComplete)
            {
                // Dialog pertama selesai → trigger door knock sequence
                StartCoroutine(DoorKnockSequence());
            }
            else
            {
                // Dialog kedua selesai → end dialogue
                EndDialogue();
            }
        }
    }

    IEnumerator DoorKnockSequence()
    {
        dialogueActive = false;

        // Hide dialogue box sementara
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[Act3Dialogue] 🚪 First dialogue complete! Starting door knock sequence...");

        // Delay sebelum ketok pintu
        yield return new WaitForSeconds(delayBeforeDoorKnock);

        // PLAY DOOR KNOCK SOUND!
        if (doorKnockSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorKnockSound);
            Debug.Log("[Act3Dialogue] 🚪 DOOR KNOCK SOUND PLAYED!");

            // Tunggu sound selesai + delay tambahan
            float soundLength = doorKnockSound.length;
            yield return new WaitForSeconds(soundLength + delayAfterDoorKnock);
        }
        else
        {
            Debug.LogWarning("[Act3Dialogue] ⚠️ Door knock sound not assigned!");
            yield return new WaitForSeconds(delayAfterDoorKnock);
        }

        // Start dialog kedua (Jannah mengira orang tuanya balik)
        Debug.Log("[Act3Dialogue] 💬 Starting continuation dialogue...");

        isFirstDialogueComplete = true;
        currentLine = 0;
        dialogueActive = true;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        StartCoroutine(TypeLine());
    }

    void EndDialogue()
    {
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Lock cursor kembali untuk gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[Act3Dialogue] All dialogues finished!");
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Manual trigger dialogue (bisa dipanggil dari script lain)
    public void TriggerDialogue()
    {
        if (!dialogueActive)
        {
            StartDialogue();
        }
    }
}