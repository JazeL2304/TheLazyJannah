using UnityEngine;

public class ChairResumeTrigger : MonoBehaviour
{
    [Header("Dialogue Manager Reference")]
    public Act2DialogueManager dialogueManager;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.F;

    private bool playerNear = false;
    private bool hasTriggered = false;

    void Start()
    {
        Debug.Log("[ChairResume] ✅ Script initialized on: " + gameObject.name);

        if (dialogueManager == null)
            Debug.LogError("[ChairResume] ❌ Dialogue Manager is NULL!");
    }

    void Update()
    {
        if (playerNear && !hasTriggered)
        {
            if (Input.GetKeyDown(interactKey))
            {
                Debug.Log("[ChairResume] 🔑 F key pressed!");
                SitAndResumeDialogue();
            }
        }
    }

    void SitAndResumeDialogue()
    {
        hasTriggered = true;

        // Resume dialogue yang di-pause
        if (dialogueManager != null)
        {
            if (dialogueManager.IsPaused())
            {
                Debug.Log("[ChairResume] ✅ Resuming paused dialogue!");
                dialogueManager.ResumeDialogue();
            }
            else
            {
                Debug.LogWarning("[ChairResume] ⚠️ Dialogue is not paused!");
            }
        }

        Debug.Log("[ChairResume] ✅ Player sat down!");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[ChairResume] 🚶 Something entered trigger! Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            playerNear = true;
            Debug.Log("[ChairResume] ✅ PLAYER DETECTED!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            Debug.Log("[ChairResume] ❌ Player left chair area!");
        }
    }
}
