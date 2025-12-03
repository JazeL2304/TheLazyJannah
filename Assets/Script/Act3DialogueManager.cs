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

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

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
                dialogueText.text = act3Dialogues[currentLine].text;
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

        DialogueLine line = act3Dialogues[currentLine];

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

        if (currentLine < act3Dialogues.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
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

        Debug.Log("[Act3Dialogue] Dialogue finished!");
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