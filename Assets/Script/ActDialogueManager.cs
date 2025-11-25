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
    public int pauseAfterLineIndex = 1; // Pause setelah line ke-2 (index 1)

    [Header("📹 CAMERA ROTATION & NPC SPAWN")]
    public Transform playerTransform; // Reference ke Player GameObject (untuk rotate player, bukan camera)
    public Camera playerCamera; // Reference ke Camera utama (untuk fallback)
    public float rotationDuration = 1f; // Durasi rotasi (detik)
    public GameObject papaNPC; // Reference ke Papa prefab/object
    public GameObject mamaNPC; // Reference ke Mama prefab/object
    public Transform npcSpawnPoint; // Posisi spawn NPC (optional)

    [Header("🎯 PAPA POSITION & ROTATION")]
    public Vector3 papaPositionOffset = new Vector3(-0.8f, 0f, 0f); // Offset dari spawn point
    public Vector3 papaRotationEuler = new Vector3(0f, 180f, 0f); // Rotation Papa (Euler angles)

    [Header("🎯 MAMA POSITION & ROTATION")]
    public Vector3 mamaPositionOffset = new Vector3(0.8f, 0f, 0f); // Offset dari spawn point
    public Vector3 mamaRotationEuler = new Vector3(0f, 180f, 0f); // Rotation Mama (Euler angles)

    [Header("💬 DIALOGUE SETELAH SPAWN")]
    public bool continueDialogueAfterSpawn = true; // Toggle untuk lanjut dialog atau tidak

    [System.Serializable]
    public class DialogueLine
    {
        public string characterName = "JANNAH";
        [TextArea(2, 5)]
        public string dialogueText;
    }

    public DialogueLine[] postSpawnDialogues; // Dialog setelah Papa & Mama spawn

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private bool isPaused = false;
    private bool isPostSpawnDialogue = false; // Flag untuk track dialog setelah spawn

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Auto-detect player transform jika belum di-assign
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("[Act2Dialogue] Player auto-detected: " + player.name);
            }
        }

        // Auto-detect camera jika belum di-assign
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // ✅ JANGAN HIDE NPCs DI START - Mereka sudah ada di Act 1
        // Kita akan teleport mereka nanti saat dialog selesai

        // Cek apakah ini Act 2
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

                // Cek apakah sedang post-spawn dialogue
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

        // ✅ DEBUG TEST - Tekan T untuk spawn NPC secara paksa
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Act2Dialogue] 🔧 MANUAL TEST - Forcing NPC spawn!");
            SpawnNPCs();
        }

        // ✅ DEBUG TEST - Tekan Y untuk rotate camera secara paksa
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

        // Pilih dialog source berdasarkan state
        string textToType;
        string speakerName;

        if (isPostSpawnDialogue && postSpawnDialogues != null && currentLine < postSpawnDialogues.Length)
        {
            textToType = postSpawnDialogues[currentLine].dialogueText;
            speakerName = postSpawnDialogues[currentLine].characterName;

            // Update nama karakter
            if (nameText != null)
            {
                nameText.text = speakerName;
            }
        }
        else
        {
            textToType = act2Dialogues[currentLine];
            // Gunakan characterName default (JANNAH)
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
        // CEK PAUSE - Pause setelah line tertentu
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

        // Cek array mana yang digunakan
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

        // ✅ PUTAR KAMERA DAN SPAWN NPC
        StartCoroutine(RotateCameraAndSpawnNPCs());
    }

    // ✅ FUNCTION BARU - Rotate camera 90 derajat ke kanan & spawn NPCs
    IEnumerator RotateCameraAndSpawnNPCs()
    {
        Debug.Log("[Act2Dialogue] 📹 Starting camera rotation & NPC spawn sequence...");

        // 1. ROTATE PLAYER (bukan camera!) 90 DERAJAT KE KANAN (SMOOTH)
        Transform targetTransform = playerTransform != null ? playerTransform :
                                   (playerCamera != null ? playerCamera.transform : null);

        if (targetTransform != null)
        {
            Quaternion startRotation = targetTransform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 90f, 0); // Rotate Y-axis 90 derajat

            float elapsedTime = 0f;

            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / rotationDuration;

                // Smooth rotation menggunakan Slerp
                targetTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            // Pastikan rotation tepat 90 derajat
            targetTransform.rotation = targetRotation;

            Debug.Log("[Act2Dialogue] ✅ " + (playerTransform != null ? "Player" : "Camera") + " rotated 90 degrees to the right!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ No transform to rotate! Assign Player Transform or Camera.");
        }

        // 2. SPAWN NPCs
        yield return new WaitForSeconds(0.3f); // Delay sebentar biar smooth

        SpawnNPCs();

        // 3. LANJUT DIALOG (jika diaktifkan)
        if (continueDialogueAfterSpawn && postSpawnDialogues != null && postSpawnDialogues.Length > 0)
        {
            yield return new WaitForSeconds(0.5f); // Delay sebelum dialog
            StartPostSpawnDialogue();
        }

        Debug.Log("[Act2Dialogue] ✅ Camera rotation & NPC spawn sequence complete!");
    }

    void SpawnNPCs()
    {
        Debug.Log("[Act2Dialogue] 🎯 SpawnNPCs() CALLED!");

        // CEK APAKAH ADA SPAWN POINT
        if (npcSpawnPoint == null)
        {
            Debug.LogError("[Act2Dialogue] ❌ NPC Spawn Point not assigned! Please assign NPCSpawnPoint in Inspector.");
            return;
        }

        // ========== TELEPORT & SPAWN PAPA ==========
        if (papaNPC != null)
        {
            Debug.Log($"[Act2Dialogue] Papa NPC found: {papaNPC.name}");

            // DISABLE MOVEMENT SCRIPTS (NPCPatrol, NPCStaticLookAround, dll)
            DisableNPCMovement(papaNPC);

            // CALCULATE POSITION dengan offset yang bisa diatur di Inspector
            Vector3 papaTargetPosition = npcSpawnPoint.position + papaPositionOffset;

            papaNPC.transform.position = papaTargetPosition;
            papaNPC.transform.rotation = Quaternion.Euler(papaRotationEuler);

            Debug.Log($"[Act2Dialogue] Papa teleported to: {papaNPC.transform.position}");
            Debug.Log($"[Act2Dialogue] Papa rotation: {papaNPC.transform.rotation.eulerAngles}");

            // AKTIFKAN NPC (jika belum aktif)
            if (!papaNPC.activeSelf)
            {
                papaNPC.SetActive(true);
                Debug.Log("[Act2Dialogue] Papa NPC activated!");
            }

            Debug.Log($"[Act2Dialogue] ✅ Papa NPC spawned successfully!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ Papa NPC not assigned! Drag 'Bapak' GameObject from Hierarchy.");
        }

        // ========== TELEPORT & SPAWN MAMA ==========
        if (mamaNPC != null)
        {
            Debug.Log($"[Act2Dialogue] Mama NPC found: {mamaNPC.name}");

            // DISABLE MOVEMENT SCRIPTS
            DisableNPCMovement(mamaNPC);

            // CALCULATE POSITION dengan offset yang bisa diatur di Inspector
            Vector3 mamaTargetPosition = npcSpawnPoint.position + mamaPositionOffset;

            mamaNPC.transform.position = mamaTargetPosition;
            mamaNPC.transform.rotation = Quaternion.Euler(mamaRotationEuler);

            Debug.Log($"[Act2Dialogue] Mama teleported to: {mamaNPC.transform.position}");
            Debug.Log($"[Act2Dialogue] Mama rotation: {mamaNPC.transform.rotation.eulerAngles}");

            // AKTIFKAN NPC (jika belum aktif)
            if (!mamaNPC.activeSelf)
            {
                mamaNPC.SetActive(true);
                Debug.Log("[Act2Dialogue] Mama NPC activated!");
            }

            Debug.Log($"[Act2Dialogue] ✅ Mama NPC spawned successfully!");
        }
        else
        {
            Debug.LogError("[Act2Dialogue] ❌ Mama NPC not assigned! Drag 'Ibu' GameObject from Hierarchy.");
        }

        Debug.Log("[Act2Dialogue] 🎯 SpawnNPCs() FINISHED!");
    }

    // ✅ FUNCTION BARU - Matikan script movement NPC
    void DisableNPCMovement(GameObject npc)
    {
        if (npc == null) return;

        // Disable NPCPatrol
        NPCPatrol patrol = npc.GetComponent<NPCPatrol>();
        if (patrol != null)
        {
            patrol.enabled = false;
            Debug.Log($"[Act2Dialogue] NPCPatrol disabled on {npc.name}");
        }

        // Disable NPCStaticLookAround
        NPCStaticLookAround lookAround = npc.GetComponent<NPCStaticLookAround>();
        if (lookAround != null)
        {
            lookAround.enabled = false;
            Debug.Log($"[Act2Dialogue] NPCStaticLookAround disabled on {npc.name}");
        }

        // Disable CharacterController (agar tidak bergerak)
        CharacterController controller = npc.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log($"[Act2Dialogue] CharacterController disabled on {npc.name}");
        }

        // Disable Animator (agar tidak animasi jalan)
        Animator animator = npc.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f; // Freeze animation
            Debug.Log($"[Act2Dialogue] Animator frozen on {npc.name}");
        }
    }

    // ✅ FUNCTION BARU - Start dialog setelah spawn
    void StartPostSpawnDialogue()
    {
        Debug.Log("[Act2Dialogue] 💬 Starting post-spawn dialogue...");

        isPostSpawnDialogue = true;
        dialogueActive = true;
        currentLine = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // Nama akan di-update di TypeLine() sesuai DialogueLine
        StartCoroutine(TypeLine());
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}