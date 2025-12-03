using UnityEngine;
using TMPro;

public class Act3ObjectiveManager : MonoBehaviour
{
    [Header("Objective UI")]
    public GameObject objectivePanel; // Panel UI objective (StealthQuestUI dari Act sebelumnya)
    public TextMeshProUGUI objectiveTitle; // Text "OBJECTIVE" atau judul quest
    public TextMeshProUGUI objectiveDescription; // Text deskripsi objective

    [Header("Door Reference")]
    public DoorInteraction targetDoor; // Pintu yang harus dibuka
    public string doorName = "Pintu Kamar"; // Nama pintu untuk ditampilkan

    [Header("Auto Start Settings")]
    public bool autoStart = false; // ← CHANGED - Manual trigger dari dialogue manager
    public float startDelay = 1f; // Delay setelah dialog selesai

    [Header("Objective Text Settings")]
    public string objectiveTitleText = "OBJECTIVE";
    public string objectiveDescriptionTemplate = "- Buka {0}"; // {0} akan diganti nama pintu

    private bool objectiveComplete = false;
    private bool objectiveStarted = false;

    void Start()
    {
        // Hide objective panel at start
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        // Auto-detect door jika tidak di-assign
        if (targetDoor == null)
        {
            targetDoor = FindObjectOfType<DoorInteraction>();
            if (targetDoor != null)
            {
                Debug.Log("[Act3Objective] Door auto-detected: " + targetDoor.gameObject.name);
            }
        }

        // Check if Act 3
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.currentAct != 3)
        {
            Debug.Log("[Act3Objective] Not Act 3 - Script disabled");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (!objectiveStarted || objectiveComplete) return;

        // Check jika pintu sudah dibuka
        if (targetDoor != null && targetDoor.isOpen)
        {
            CompleteObjective();
        }
    }

    public void StartObjective()
    {
        objectiveStarted = true;

        Debug.Log("[Act3Objective] Objective dimulai: Buka pintu!");

        // Show objective panel
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }

        // Set objective title
        if (objectiveTitle != null)
        {
            objectiveTitle.text = objectiveTitleText;
        }

        // Set objective description
        if (objectiveDescription != null)
        {
            objectiveDescription.text = string.Format(objectiveDescriptionTemplate, doorName);
        }

        Debug.Log("[Act3Objective] Objective UI displayed!");
    }

    void CompleteObjective()
    {
        if (objectiveComplete) return;

        objectiveComplete = true;

        Debug.Log("[Act3Objective] ✅ Objective complete! Pintu berhasil dibuka!");

        // Update objective text
        if (objectiveDescription != null)
        {
            objectiveDescription.text = $"✓ {doorName} dibuka"; // Checkmark + status
        }

        // Auto-hide panel setelah beberapa detik (opsional)
        Invoke("HideObjectivePanel", 2f);
    }

    void HideObjectivePanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        Debug.Log("[Act3Objective] Objective panel hidden!");
    }

    // Manual trigger objective (bisa dipanggil dari script lain)
    public void TriggerObjective()
    {
        if (!objectiveStarted)
        {
            StartObjective();
        }
    }

    // Manual complete objective (untuk testing)
    public void ForceCompleteObjective()
    {
        CompleteObjective();
    }

    public bool IsObjectiveComplete()
    {
        return objectiveComplete;
    }

    public bool IsObjectiveStarted()
    {
        return objectiveStarted;
    }
}