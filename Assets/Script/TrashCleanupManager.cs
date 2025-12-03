using UnityEngine;
using TMPro;

public class TrashCleanupManager : MonoBehaviour
{
    [Header("Trash Objects")]
    public GameObject[] trashObjects; // Array sampah yang harus diberesin

    [Header("Quest UI")]
    public GameObject questUI;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 3f;
    public GameObject interactionPrompt; // UI "[E] Beresin"

    [Header("Parent NPC Settings")]
    public ParentNPCInteraction parentNPC; // Reference ke script parent

    private int trashedCleaned = 0;
    private int totalTrash;
    private bool questActive = false;
    private GameObject nearestTrash = null;

    void Start()
    {
        totalTrash = trashObjects.Length;

        // Hide UI at start
        if (questUI != null)
        {
            questUI.SetActive(false);
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Setup trash objects & DISABLE COLLIDERS di awal
        foreach (GameObject trash in trashObjects)
        {
            if (trash != null)
            {
                // Add tag untuk identifikasi
                if (!trash.CompareTag("Trash"))
                {
                    trash.tag = "Trash";
                }

                // DISABLE COLLIDER di Act 1 - sampah cuma dekorasi
                Collider col = trash.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        Debug.Log($"[TrashCleanup] Total sampah: {totalTrash}");
        Debug.Log("[TrashCleanup] Sampah colliders disabled - menunggu cleanup quest dimulai!");
    }

    void Update()
    {
        if (!questActive) return;

        // Check nearest trash
        CheckNearestTrash();

        // Interact with trash
        if (Input.GetKeyDown(interactKey) && nearestTrash != null)
        {
            CleanTrash(nearestTrash);
        }
    }

    public void StartCleanupQuest()
    {
        questActive = true;
        trashedCleaned = 0;

        // ENABLE COLLIDERS - sampah bisa di-interact sekarang!
        foreach (GameObject trash in trashObjects)
        {
            if (trash != null)
            {
                Collider col = trash.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }

        // Show quest UI
        if (questUI != null)
        {
            questUI.SetActive(true);
        }

        UpdateQuestUI();

        Debug.Log("[TrashCleanup] Quest dimulai! Beresin semua sampah!");
        Debug.Log("[TrashCleanup] Sampah colliders ENABLED - bisa di-interact!");
    }

    void CheckNearestTrash()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        nearestTrash = null;
        float closestDistance = interactionRange;

        foreach (GameObject trash in trashObjects)
        {
            if (trash == null || !trash.activeInHierarchy) continue;

            float distance = Vector3.Distance(player.transform.position, trash.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTrash = trash;
            }
        }

        // Show/hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(nearestTrash != null);
        }
    }

    void CleanTrash(GameObject trash)
    {
        if (trash == null) return;

        // Hide/destroy trash
        trash.SetActive(false);
        trashedCleaned++;

        Debug.Log($"[TrashCleanup] Sampah diberesin! ({trashedCleaned}/{totalTrash})");

        // Update UI
        UpdateQuestUI();

        // Check if all trash cleaned
        if (trashedCleaned >= totalTrash)
        {
            CompleteCleanupQuest();
        }
    }

    void UpdateQuestUI()
    {
        if (questTitle != null)
        {
            questTitle.text = "Objective";
        }

        if (questDescription != null)
        {
            questDescription.text = $"- Sampah diberesin: {trashedCleaned}/{totalTrash}";
        }
    }

    void CompleteCleanupQuest()
    {
        questActive = false;

        Debug.Log("[TrashCleanup] Semua sampah sudah diberesin!");

        // Hide quest UI (atau ubah jadi "SELESAI")
        if (questDescription != null)
        {
            questDescription.text = "KAMAR SUDAH BERSIH!\nBicara dengan Papa [E]";
        }

        // Aktifkan parent NPC untuk bisa di-interact
        if (parentNPC != null)
        {
            parentNPC.EnableInteraction();
        }
        else
        {
            Debug.LogWarning("[TrashCleanup] ParentNPC reference not set!");
        }
    }

    public bool IsQuestComplete()
    {
        return trashedCleaned >= totalTrash;
    }

    public bool IsQuestActive()
    {
        return questActive;
    }
}   