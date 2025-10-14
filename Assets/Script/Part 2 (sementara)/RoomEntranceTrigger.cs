using UnityEngine;

public class RoomEntranceTrigger : MonoBehaviour
{
    [Header("Dialogue Integration")]
    public DialogueRoomManager dialogueRoomManager;  // TAMBAHAN BARU - Untuk trigger dialog
    public bool useDialogue = true;  // TAMBAHAN BARU - Toggle pakai dialog atau langsung complete

    [Header("Quest Integration")]
    public QuestManager questManager;
    public int questIndex = 0;
    public int objectiveIndex = 0;  // Objective 1 - Pergi ke kamar

    [Header("Door Settings")]
    public DoorInteraction door;  // Reference ke pintu kamar
    public bool autoCloseDoor = true;
    public float closeDelay = 1f;  // Delay sebelum tutup pintu (detik)

    [Header("Tutorial/Dialog")]
    public GameObject tutorialUI;  // Optional: UI hint "Cari dompet di lemari"
    public float tutorialDisplayTime = 3f;

    private bool hasTriggered = false;

    void Start()
    {
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }

        // AUTO-DETECT DIALOGUEROOMMANAGER - TAMBAHAN BARU
        if (useDialogue && dialogueRoomManager == null)
        {
            dialogueRoomManager = FindObjectOfType<DialogueRoomManager>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            Debug.Log("Player masuk kamar orang tua!");

            // JIKA PAKAI DIALOG - TAMBAHAN BARU
            if (useDialogue && dialogueRoomManager != null)
            {
                Debug.Log("Trigger dialog kamar...");

                // Start dialog (objective akan di-complete setelah dialog selesai)
                dialogueRoomManager.StartRoomDialogue();

                // DialogueRoomManager akan handle:
                // - Complete objective
                // - Close door
                // Jadi kita skip logic di bawah
                return;
            }

            // FALLBACK: Jika tidak pakai dialog, logic original tetap jalan
            // Complete Objective 1
            if (questManager != null && !questManager.IsObjectiveComplete(questIndex, objectiveIndex))
            {
                questManager.CompleteCurrentObjective();
                Debug.Log("Objective 1 complete! Objective 2 dimulai.");
            }

            // Auto-close door
            if (autoCloseDoor && door != null)
            {
                Invoke("CloseDoor", closeDelay);
            }

            // Show tutorial hint
            if (tutorialUI != null)
            {
                ShowTutorial();
            }
        }
    }

    void CloseDoor()
    {
        if (door != null && door.isOpen)
        {
            door.InteractWithDoor();  // Tutup pintu
            Debug.Log("Pintu kamar tertutup otomatis.");
        }
    }

    void ShowTutorial()
    {
        tutorialUI.SetActive(true);
        Invoke("HideTutorial", tutorialDisplayTime);
    }

    void HideTutorial()
    {
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }
    }
}
