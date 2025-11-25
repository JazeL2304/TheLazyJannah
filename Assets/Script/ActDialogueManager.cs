using UnityEngine;
using TMPro;
using System.Collections;

public class Act2DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    public string characterName = "JANNAH";
    public float textSpeed = 0.05f;

    [Header("⚙️ Trigger Settings")]
    public float startDelay = 0.5f;

    [Header("🔊 Audio Settings")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    [Header("Act 2 Dialogue")]
    [TextArea(3, 10)]
    public string[] act2Dialogues;

    [Header("🛑 Pause Settings")]
    public int pauseAfterLineIndex = 1;

    [Header("📹 CAMERA ROTATION & NPC SPAWN")]
    public Transform playerTransform;
    public Camera playerCamera;
    public float rotationDuration = 1f;
    public GameObject papaNPC;
    public GameObject mamaNPC;
    public Transform npcSpawnPoint;

    [Header("🚫 ACT 1 NPCs TO DISABLE")]
    public GameObject[] act1NPCsToDisable; // Papa & Mama dari Act 1 (yang di dapur)

    [Header("🎯 PAPA POSITION & ROTATION")]
    public Vector3 papaPositionOffset = new Vector3(-0.8f, 0f, 0f);
    public Vector3 papaRotationEuler = new Vector3(0f, 180f, 0f);

    [Header("🎯 MAMA POSITION & ROTATION")]
    public Vector3 mamaPositionOffset = new Vector3(0.8f, 0f, 0f);
    public Vector3 mamaRotationEuler = new Vector3(0f, 180f, 0f);

    [Header("💬 DIALOGUE SETELAH SPAWN")]
    public bool continueDialogueAfterSpawn = true;

    [System.Serializable]
    public class DialogueLine
    {
        public string characterName = "JANNAH";
        [TextArea(2, 5)]
        public string dialogueText;
    }

    public DialogueLine[] postSpawnDialogues;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isPaused = false;
    private bool isPostSpawnDialogue = false;
    private bool hasSpawnedNPCs = false; // ✅ FLAG BARU - Cegah spawn berulang!

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("[Act2Dialogue] Player auto-detected: " + player.name);
            }
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // ✅ DISABLE ACT 1 NPCs (Papa & Mama dari dapur)
        DisableAct1NPCs();

        if (GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;

            if (currentAct == 2)
            {
                if (dialogueBox != null)
                {
                    dialogueBox.SetActive(false);
                }

                StartCoroutine(AutoStartDialogue());
            }
            else
            {
                Debug.Log("[Act2Dialogue] Not Act 2 - Script disabled.");
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
        if (!dialogueActive || isPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();

            if (isTyping)
            {
                StopAllCoroutines();

                if (isPostSpawnDialogue && postSpawnDialogues != null && currentLine < postSpawnDialogues.Length)
                {
                    dialogueText.text = postSpawnDialogues[currentLine].dialogueText;
                }
                else
                {
                    dialogueText.text = act2Dialogues[currentLine];
                }

                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }

        // DEBUG TEST
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Act2Dialogue] 🔧 MANUAL TEST - Forcing NPC spawn!");
            SpawnNPCs();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[Act2Dialogue] 🔧 MANUAL TEST - Forcing camera rotation!");
            StartCoroutine(RotateCameraAndSpawnNPCs());
        }
    }

    public void StartDialogue()
    {
        dialogueActive = true;
        isPaused = false;
        currentLine = 0;
        isPostSpawnDialogue = false; // ✅ RESET FLAG

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = characterName;
        }

        if (act2Dialogues != null && act2Dialogues.Length > 0)
        {
            StartCoroutine(TypeLine());
        }

        Debug.Log("[Act2Dialogue] Dialogue started!");
    }

    public void ResumeDialogue()
    {
        if (isPaused)
        {
            Debug.Log("[Act2Dialogue] ✅ Resuming dialogue from line " + (currentLine + 1));
            isPaused = false;
            dialogueActive = true;

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }

            currentLine++;
            StartCoroutine(TypeLine());
        }
        else
        {
            Debug.LogWarning("[Act2Dialogue] ⚠️ Dialogue is not paused!");
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        string textToType;
        string speakerName;

        if (isPostSpawnDialogue && postSpawnDialogues != null && currentLine < postSpawnDialogues.Length)
        {
            textToType = postSpawnDialogues[currentLine].dialogueText;
            speakerName = postSpawnDialogues[currentLine].characterName;

            if (nameText != null)
            {
                nameText.text = speakerName;
            }
        }
        else
        {
            textToType = act2Dialogues[currentLine];
            if (nameText != null)
            {
                nameText.text = characterName;
            }
        }

        foreach (char c in textToType.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        // CEK PAUSE
        if (!isPostSpawnDialogue && currentLine == pauseAfterLineIndex)
        {
            isPaused = true;
            dialogueActive = false;

            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }

            Debug.Log("[Act2Dialogue] 🛑 Dialogue PAUSED after line " + (currentLine + 1));
            return;
        }

        currentLine++;

        int maxLines = isPostSpawnDialogue ?
            (postSpawnDialogues != null ? postSpawnDialogues.Length : 0) :
            act2Dialogues.Length;

        if (currentLine < maxLines)
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
        isPaused = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        Debug.Log("[Act2Dialogue] Dialogue finished!");

        // ✅ FIX UTAMA - Cek apakah ini dialog pertama atau kedua
        if (!isPostSpawnDialogue && !hasSpawnedNPCs)
        {
            // Dialog PERTAMA selesai → Spawn NPCs & start dialog kedua
            Debug.Log("[Act2Dialogue] 📹 First dialogue done - Starting spawn sequence...");
            StartCoroutine(RotateCameraAndSpawnNPCs());
        }
        else if (isPostSpawnDialogue)
        {
            // Dialog KEDUA selesai → SELESAI TOTAL!
            Debug.Log("[Act2Dialogue] ✅ Post-spawn dialogue FINISHED - Act 2 dialogue complete!");
            // Tidak ada yang dilakukan lagi - dialog benar-benar selesai
        }
    }

    IEnumerator RotateCameraAndSpawnNPCs()
    {
        // ✅ PREVENT DOUBLE EXECUTION
        if (hasSpawnedNPCs)
        {
            Debug.LogWarning("[Act2Dialogue] ⚠️ NPCs already spawned! Skipping...");
            yield break;
        }

        hasSpawnedNPCs = true; // Set flag SEBELUM proses

        Debug.Log("[Act2Dialogue] 📹 Starting camera rotation & NPC spawn sequence...");

        Transform targetTransform = playerTransform != null ? playerTransform :
                                   (playerCamera != null ? playerCamera.transform : null);

        if (targetTransform != null)
        {
            Quaternion startRotation = targetTransform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 90f, 0);

            float elapsedTime = 0f;

            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / rotationDuration;
                targetTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            targetTransform.rotation = targetRotation;
            Debug.Log("[Act2Dialogue] ✅ " + (playerTransform != null ? "Player" : "Camera") + " rotated 90 degrees to the right!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ No transform to rotate! Assign Player Transform or Camera.");
        }

        yield return new WaitForSeconds(0.3f);

        SpawnNPCs();

        if (continueDialogueAfterSpawn && postSpawnDialogues != null && postSpawnDialogues.Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
            StartPostSpawnDialogue();
        }

        Debug.Log("[Act2Dialogue] ✅ Camera rotation & NPC spawn sequence complete!");
    }

    void SpawnNPCs()
    {
        Debug.Log("[Act2Dialogue] 🎯 SpawnNPCs() CALLED!");

        if (npcSpawnPoint == null)
        {
            Debug.LogError("[Act2Dialogue] ❌ NPC Spawn Point not assigned!");
            return;
        }

        // PAPA
        if (papaNPC != null)
        {
            Debug.Log($"[Act2Dialogue] Papa NPC found: {papaNPC.name}");
            DisableNPCMovement(papaNPC);

            Vector3 papaTargetPosition = npcSpawnPoint.position + papaPositionOffset;
            papaNPC.transform.position = papaTargetPosition;
            papaNPC.transform.rotation = Quaternion.Euler(papaRotationEuler);

            Debug.Log($"[Act2Dialogue] Papa teleported to: {papaNPC.transform.position}");

            if (!papaNPC.activeSelf)
            {
                papaNPC.SetActive(true);
                Debug.Log("[Act2Dialogue] Papa NPC activated!");
            }

            Debug.Log($"[Act2Dialogue] ✅ Papa NPC spawned successfully!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ Papa NPC not assigned!");
        }

        // MAMA
        if (mamaNPC != null)
        {
            Debug.Log($"[Act2Dialogue] Mama NPC found: {mamaNPC.name}");
            DisableNPCMovement(mamaNPC);

            Vector3 mamaTargetPosition = npcSpawnPoint.position + mamaPositionOffset;
            mamaNPC.transform.position = mamaTargetPosition;
            mamaNPC.transform.rotation = Quaternion.Euler(mamaRotationEuler);

            Debug.Log($"[Act2Dialogue] Mama teleported to: {mamaNPC.transform.position}");

            if (!mamaNPC.activeSelf)
            {
                mamaNPC.SetActive(true);
                Debug.Log("[Act2Dialogue] Mama NPC activated!");
            }

            Debug.Log($"[Act2Dialogue] ✅ Mama NPC spawned successfully!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ Mama NPC not assigned!");
        }

        Debug.Log("[Act2Dialogue] 🎯 SpawnNPCs() FINISHED!");
    }

    void DisableNPCMovement(GameObject npc)
    {
        if (npc == null) return;

        NPCPatrol patrol = npc.GetComponent<NPCPatrol>();
        if (patrol != null)
        {
            patrol.enabled = false;
            Debug.Log($"[Act2Dialogue] NPCPatrol disabled on {npc.name}");
        }

        NPCStaticLookAround lookAround = npc.GetComponent<NPCStaticLookAround>();
        if (lookAround != null)
        {
            lookAround.enabled = false;
            Debug.Log($"[Act2Dialogue] NPCStaticLookAround disabled on {npc.name}");
        }

        CharacterController controller = npc.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log($"[Act2Dialogue] CharacterController disabled on {npc.name}");
        }

        Animator animator = npc.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
            Debug.Log($"[Act2Dialogue] Animator frozen on {npc.name}");
        }
    }

    void StartPostSpawnDialogue()
    {
        Debug.Log("[Act2Dialogue] 💬 Starting post-spawn dialogue...");

        isPostSpawnDialogue = true; // ✅ SET FLAG
        dialogueActive = true;
        currentLine = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        StartCoroutine(TypeLine());
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // ✅ FUNCTION BARU - Disable NPCs dari Act 1
    void DisableAct1NPCs()
    {
        if (act1NPCsToDisable == null || act1NPCsToDisable.Length == 0)
        {
            Debug.LogWarning("[Act2Dialogue] No Act 1 NPCs assigned to disable");
            return;
        }

        foreach (GameObject npc in act1NPCsToDisable)
        {
            if (npc != null)
            {
                npc.SetActive(false);
                Debug.Log($"[Act2Dialogue] ✅ Disabled Act 1 NPC: {npc.name}");
            }
        }

        Debug.Log($"[Act2Dialogue] Total {act1NPCsToDisable.Length} Act 1 NPCs disabled");
    }
}