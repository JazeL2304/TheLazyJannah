using UnityEngine;
using TMPro;
using System.Collections;

public class Act3ChaseManager : MonoBehaviour
{
    [Header("🏃 CHASE SETTINGS")]
    public GameObject debtCollectorNPC;
    public Transform player;
    public float chaseSpeed = 4f;
    public float catchDistance = 1.5f; // Harus sangat dekat untuk caught
    public float chaseStartDelay = 1f;

    [Header("🎯 OBJECTIVE UI")]
    public GameObject objectivePanel;
    public TextMeshProUGUI objectiveTitle;
    public TextMeshProUGUI objectiveDescription;
    public string objectiveTitleText = "OBJECTIVE";
    public string objectiveDescriptionText = "- KABUR DARI DEBT COLLECTOR!";

    [Header("💬 CAUGHT DIALOGUE")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueText;
    public float textSpeed = 0.05f;

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker = "DEBT COLLECTOR";
        [TextArea(2, 5)]
        public string text;
    }

    [Header("📝 CAUGHT DIALOGUE LINES")]
    public DialogueLine[] caughtDialogues = new DialogueLine[]
    {
        new DialogueLine { speaker = "DEBT COLLECTOR", text = "Tertangkap! Kamu tidak bisa lari dariku!" },
        new DialogueLine { speaker = "JANNAH", text = "Tidak! Lepaskan aku!" },
        new DialogueLine { speaker = "DEBT COLLECTOR", text = "Sudah terlambat. Kamu harus ikut denganku sekarang." },
        new DialogueLine { speaker = "JANNAH", text = "TOLONG! ADA YANG BISA BANTU?!" },
        new DialogueLine { speaker = "DEBT COLLECTOR", text = "Tidak ada yang akan menolongmu. Ayo, kita pergi." }
    };

    [Header("🎬 BAD ENDING PANEL")]
    public GameObject badEndingPanel;
    public float badEndingDelay = 2f;

    [Header("🔊 AUDIO")]
    public AudioClip clickSound;
    public AudioClip caughtSound;
    public AudioClip chaseMusic;
    private AudioSource audioSource;

    [Header("⚙️ CHASE STATE")]
    private bool isChasing = false;
    private bool isCaught = false;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(false);
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        if (debtCollectorNPC != null)
        {
            debtCollectorNPC.SetActive(false);
        }

        Debug.Log("[Act3Chase] Chase Manager initialized!");
    }

    void Update()
    {
        if (isCaught)
        {
            // Handle dialogue input
            if (dialogueActive && Input.GetMouseButtonDown(0))
            {
                PlayClickSound();

                if (isTyping)
                {
                    StopAllCoroutines();
                    dialogueText.text = caughtDialogues[currentLine].text;
                    isTyping = false;
                }
                else
                {
                    NextLine();
                }
            }
            return;
        }

        if (!isChasing || player == null || debtCollectorNPC == null) return;

        // Chase player
        Vector3 direction = (player.position - debtCollectorNPC.transform.position).normalized;
        debtCollectorNPC.transform.position += direction * chaseSpeed * Time.deltaTime;

        // Look at player
        Vector3 lookDirection = player.position - debtCollectorNPC.transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            debtCollectorNPC.transform.rotation = Quaternion.Slerp(
                debtCollectorNPC.transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Time.deltaTime * 5f
            );
        }

        // Check if caught - HARUS SANGAT DEKAT!
        float distance = Vector3.Distance(player.position, debtCollectorNPC.transform.position);

        // Debug untuk monitor jarak
        if (distance <= catchDistance + 0.5f) // Warning range
        {
            Debug.Log($"[Act3Chase] ⚠️ DANGER! Distance: {distance:F2}m | Catch at: {catchDistance}m");
        }

        if (distance <= catchDistance)
        {
            OnPlayerCaught();
        }
    }

    public void StartChase()
    {
        StartCoroutine(StartChaseSequence());
    }

    IEnumerator StartChaseSequence()
    {
        Debug.Log("[Act3Chase] Starting chase sequence...");

        yield return new WaitForSeconds(chaseStartDelay);

        isChasing = true;

        // Show objective panel
        ShowObjective();

        // Play chase music
        if (chaseMusic != null && audioSource != null)
        {
            audioSource.clip = chaseMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log("[Act3Chase] 🏃 CHASE STARTED! RUN!");
    }

    void OnPlayerCaught()
    {
        if (isCaught) return;

        isCaught = true;
        isChasing = false;

        Debug.Log("[Act3Chase] 😱 PLAYER CAUGHT!");

        // Hide objective panel
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        // Stop chase music
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Play caught sound
        if (caughtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(caughtSound);
        }

        // Disable player movement
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        // Start caught dialogue
        StartCaughtDialogue();
    }

    void StartCaughtDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        DialogueLine line = caughtDialogues[currentLine];

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

        if (currentLine < caughtDialogues.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndCaughtDialogue();
        }
    }

    void EndCaughtDialogue()
    {
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[Act3Chase] Caught dialogue finished! Showing bad ending...");

        // Show bad ending panel
        StartCoroutine(ShowBadEnding());
    }

    IEnumerator ShowBadEnding()
    {
        yield return new WaitForSeconds(badEndingDelay);

        if (badEndingPanel != null)
        {
            badEndingPanel.SetActive(true);
            Debug.Log("[Act3Chase] 💀 BAD ENDING DISPLAYED!");
        }
        else
        {
            Debug.LogError("[Act3Chase] Bad Ending Panel not assigned!");
        }
    }

    void ShowObjective()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);

            if (objectiveTitle != null)
            {
                objectiveTitle.text = objectiveTitleText;
            }

            if (objectiveDescription != null)
            {
                objectiveDescription.text = objectiveDescriptionText;
            }

            Debug.Log("[Act3Chase] ✅ Objective panel shown: KABUR!");
        }
        else
        {
            Debug.LogWarning("[Act3Chase] ⚠️ Objective Panel not assigned!");
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void ManualStartChase()
    {
        StartChase();
    }

    public bool IsChasing()
    {
        return isChasing;
    }

    public bool IsCaught()
    {
        return isCaught;
    }
}