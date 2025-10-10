using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPosition;
    public Vector3 sitOffset = new Vector3(0, 0.5f, 0);
    public GameObject interactionUI;

    [Header("Dialogue Settings")]
    public Dialogue dialogueManager;
    public bool triggerDialogueOnSit = false;

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