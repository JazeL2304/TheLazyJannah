using UnityEngine;
using TMPro;
using System.Collections;

public class Act3DoorEventManager : MonoBehaviour
{
    [Header("🚪 DOOR REFERENCE")]
    public DoorInteraction targetDoor;

    [Header("👻 CHARACTER SPAWN SETTINGS")]
    public GameObject mysteriousCharacter;
    public Transform spawnPoint;
    public bool characterStartsHidden = true;

    [Header("⏱️ SPAWN TIMING")]
    public float delayBeforeSpawn = 0.5f;
    public float fadeInDuration = 1f;

    [Header("💬 DIALOGUE AFTER SPAWN")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [Header("🔊 AUDIO SETTINGS")]
    public AudioClip doorOpenSound;
    public AudioClip characterAppearSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker = "???";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("📝 CHARACTER DIALOGUE")]
    public DialogueLine[] characterDialogues = new DialogueLine[]
    {
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Jannah, bukan?"
        },
        new DialogueLine {
            speaker = "JANNAH",
            text = "S-siapa kamu?! Kenapa masuk ke kamarku?!"
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Aku debt collector. Orang tuamu punya utang yang harus dibayar."
        },
        new DialogueLine {
            speaker = "JANNAH",
            text = "Utang? Apa maksudmu? Dan... dimana Mama Papa?!"
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Mereka sudah tidak ada. Mereka kabur meninggalkan utang mereka."
        },
        new DialogueLine {
            speaker = "JANNAH",
            text = "Tidak mungkin! Mereka... mereka tidak mungkin pergi begitu saja!"
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Oh, mereka pergi. Dan meninggalkanmu sendirian dengan semua utang ini."
        },
        new DialogueLine {
            speaker = "JANNAH",
            text = "Tapi... aku tidak tahu apa-apa tentang utang mereka!"
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Tidak peduli. Seseorang harus membayar. Dan karena mereka tidak ada..."
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Kalau begitu, aku harus membawa kamu. Kamu akan bayar utang orang tuamu."
        },
        new DialogueLine {
            speaker = "JANNAH",
            text = "APA?! TIDAK! JANGAN SENTUH AKU!"
        },
        new DialogueLine {
            speaker = "DEBT COLLECTOR",
            text = "Maaf, anak kecil. Ini bukan pilihan. Ikut aku... atau lebih parah."
        }
    };

    [Header("🎯 OBJECTIVE COMPLETION")]
    public Act3ObjectiveManager objectiveManager;

    [Header("🏃 CHASE SYSTEM")]
    public Act3ChaseManager chaseManager;
    public bool startChaseAfterDialogue = true;

    private bool doorOpened = false;
    private bool characterSpawned = false;
    private bool dialogueActive = false;
    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (targetDoor == null)
        {
            targetDoor = FindObjectOfType<DoorInteraction>();
            if (targetDoor != null)
            {
                Debug.Log("[Act3DoorEvent] Door auto-detected!");
            }
        }

        if (objectiveManager == null)
        {
            objectiveManager = FindObjectOfType<Act3ObjectiveManager>();
        }

        if (chaseManager == null)
        {
            chaseManager = FindObjectOfType<Act3ChaseManager>();
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (mysteriousCharacter != null && characterStartsHidden)
        {
            mysteriousCharacter.SetActive(false);
            Debug.Log("[Act3DoorEvent] Character hidden at start");
        }

        Debug.Log("[Act3DoorEvent] Script initialized!");
    }

    void Update()
    {
        if (!doorOpened && targetDoor != null && targetDoor.isOpen)
        {
            OnDoorOpened();
        }

        if (dialogueActive && Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = characterDialogues[currentLine].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    void OnDoorOpened()
    {
        doorOpened = true;

        Debug.Log("[Act3DoorEvent] 🚪 PINTU DIBUKA! Starting spawn sequence...");

        if (targetDoor != null && targetDoor.interactionUI != null)
        {
            targetDoor.interactionUI.SetActive(false);
            Debug.Log("[Act3DoorEvent] ✅ Door interaction UI disabled!");
        }

        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }

        StartCoroutine(SpawnCharacterSequence());
    }

    IEnumerator SpawnCharacterSequence()
    {
        if (objectiveManager != null)
        {
            objectiveManager.ForceCompleteObjective();
        }

        yield return new WaitForSeconds(delayBeforeSpawn);

        Debug.Log("[Act3DoorEvent] 👻 SPAWNING CHARACTER...");

        if (mysteriousCharacter != null)
        {
            if (spawnPoint != null)
            {
                mysteriousCharacter.transform.position = spawnPoint.position;
                mysteriousCharacter.transform.rotation = spawnPoint.rotation;
            }

            mysteriousCharacter.SetActive(true);

            if (characterAppearSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(characterAppearSound);
            }

            Debug.Log("[Act3DoorEvent] ✅ Character spawned at: " + mysteriousCharacter.transform.position);
        }
        else
        {
            Debug.LogError("[Act3DoorEvent] ❌ Mysterious Character not assigned!");
        }

        characterSpawned = true;

        yield return new WaitForSeconds(1f);

        StartDialogue();
    }

    void StartDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        Debug.Log("[Act3DoorEvent] 💬 Starting character dialogue...");

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log("[Act3DoorEvent] Player movement disabled for dialogue");
            }
        }

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        DialogueLine line = characterDialogues[currentLine];

        if (dialogueNameText != null)
        {
            dialogueNameText.text = line.speaker;
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

        if (currentLine < characterDialogues.Length)
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

        Debug.Log("[Act3DoorEvent] ✅ Dialogue finished!");

        // START CHASE SYSTEM!
        if (startChaseAfterDialogue && chaseManager != null)
        {
            Debug.Log("[Act3DoorEvent] 🏃 STARTING CHASE SYSTEM!");

            // Re-enable player movement for chase
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = true;
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            chaseManager.StartChase();
        }
        else if (startChaseAfterDialogue && chaseManager == null)
        {
            Debug.LogError("[Act3DoorEvent] ❌ Chase Manager not assigned!");
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void ManualTriggerSpawn()
    {
        if (!characterSpawned)
        {
            OnDoorOpened();
        }
    }
}