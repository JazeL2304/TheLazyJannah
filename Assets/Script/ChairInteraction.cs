using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPosition;
    public Vector3 sitOffset = new Vector3(0, 0.5f, 0);
    public GameObject interactionUI;

    [Header("Dialogue Settings")]
    public Dialogue dialogueManager;
    public bool triggerDialogueOnSit = false;

    [Header("🚪 ACT 2 DOOR KNOCK SETTINGS")]
    public Act2DialogueManager act2DialogueManager;  // ← BARU
    public AudioClip doorKnockSound;  // ← BARU
    public float doorKnockDelay = 0.5f;  // ← BARU
    private AudioSource audioSource;  // ← BARU

    void Start()
    {
        if (sitPosition == null)
        {
            GameObject sitPos = new GameObject("SitPosition");
            sitPos.transform.parent = transform;
            sitPos.transform.localPosition = sitOffset;
            sitPos.transform.localRotation = Quaternion.identity;
            sitPosition = sitPos.transform;
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // ← BARU: Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ← BARU: Auto-detect Act2DialogueManager
        if (act2DialogueManager == null)
        {
            act2DialogueManager = FindObjectOfType<Act2DialogueManager>();
        }

        // Validasi
        if (triggerDialogueOnSit)
        {
            if (dialogueManager == null)
            {
                Debug.LogError("ERROR: Trigger Dialogue On Sit aktif, tapi Dialogue Manager NULL!");
            }
            else
            {
                Debug.Log("ChairInteraction OK. Dialogue Manager: " + dialogueManager.gameObject.name);
            }
        }
    }

    public void InteractWithChair(PlayerChairController playerController)
    {
        if (playerController != null && !playerController.IsSitting())
        {
            Debug.Log("=== PLAYER DUDUK DI KURSI ===");

            playerController.SitOnChair(sitPosition);
            Debug.Log("Player duduk!");

            // ← BARU: CEK APAKAH PERLU PLAY DOOR KNOCK SOUND
            if (ShouldPlayDoorKnock())
            {
                StartCoroutine(PlayDoorKnockAfterDelay());
            }

            // TRIGGER RESUME DIALOG
            if (triggerDialogueOnSit)
            {
                Debug.Log("Trigger Dialogue On Sit: AKTIF");

                if (dialogueManager != null)
                {
                    Debug.Log("Dialogue Manager found: " + dialogueManager.gameObject.name);

                    if (dialogueManager.IsPaused())
                    {
                        Debug.Log("Dialog sedang PAUSE. Mencoba resume...");
                        dialogueManager.ResumeDialogue();
                        Debug.Log("ResumeDialogue() berhasil dipanggil!");
                    }
                    else
                    {
                        Debug.LogWarning("Dialog TIDAK sedang pause!");
                    }
                }
                else
                {
                    Debug.LogError("Dialogue Manager NULL!");
                }
            }
            else
            {
                Debug.Log("Trigger Dialogue On Sit: TIDAK AKTIF");
            }

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }

            Debug.Log("=== SELESAI DUDUK ===");
        }
    }

    // ← BARU: Function untuk cek apakah harus play door knock
    bool ShouldPlayDoorKnock()
    {
        // 1. CEK APAKAH SEDANG DI ACT 2
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.currentAct != 2)
        {
            Debug.Log("[ChairInteraction] Not Act 2 - No door knock");
            return false;
        }

        // 2. CEK APAKAH ACT 2 DIALOGUE MANAGER ADA DAN VALID
        if (act2DialogueManager == null)
        {
            Debug.Log("[ChairInteraction] Act2DialogueManager not found - No door knock");
            return false;
        }

        // 3. CEK APAKAH DIALOG SEDANG PAUSE (MENUNGGU PLAYER DUDUK)
        if (!act2DialogueManager.IsPaused())
        {
            Debug.Log("[ChairInteraction] Act2 dialogue not paused - No door knock");
            return false;
        }

        // 4. SEMUA KONDISI TERPENUHI!
        Debug.Log("[ChairInteraction] ✅ All conditions met - Will play door knock!");
        return true;
    }

    // ← BARU: Coroutine untuk play door knock dengan delay
    System.Collections.IEnumerator PlayDoorKnockAfterDelay()
    {
        yield return new WaitForSeconds(doorKnockDelay);

        if (doorKnockSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorKnockSound);
            Debug.Log("[ChairInteraction] 🚪 DOOR KNOCK SOUND PLAYED!");
        }
        else
        {
            Debug.LogWarning("[ChairInteraction] ⚠️ Door knock sound not assigned!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player terdeteksi di dekat kursi!");

            PlayerChairController playerController = other.GetComponent<PlayerChairController>();
            PlayerController controller = other.GetComponent<PlayerController>();

            if (controller != null)
            {
                controller.SetCurrentChair(this);
            }

            if (playerController != null && !playerController.IsSitting())
            {
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player keluar dari area kursi.");

            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetCurrentChair(null);
            }

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    public Transform GetSitPosition()
    {
        return sitPosition;
    }
}