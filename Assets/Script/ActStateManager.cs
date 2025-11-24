using UnityEngine;

public class ActStateManager : MonoBehaviour
{
    [Header("Act 1 Exclusive Objects")]
    public GameObject[] act1Objects; // Dialogue & object yang CUMA ada di Act 1

    [Header("Act 2 Exclusive Objects")]
    public GameObject[] act2Objects; // Dialogue & object yang CUMA ada di Act 2

    [Header("Act 3 Exclusive Objects")]
    public GameObject[] act3Objects; // Dialogue & object yang CUMA ada di Act 3

    void Start()
    {
        SetupAct();
    }

    void SetupAct()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[ActState] GameProgressManager not found! Using default setup.");
            return;
        }

        int currentAct = GameProgressManager.Instance.currentAct;

        // Act 1: Show Act 1 objects, hide Act 2 & 3 objects
        if (currentAct == 1)
        {
            SetObjectsActive(act1Objects, true);
            SetObjectsActive(act2Objects, false);
            SetObjectsActive(act3Objects, false);
            Debug.Log($"[ActState] ✅ ACT 1: Act1 objects SHOWN, Act2 & Act3 HIDDEN");
        }
        // Act 2: Hide Act 1 objects, show Act 2 objects
        else if (currentAct == 2)
        {
            SetObjectsActive(act1Objects, false);
            SetObjectsActive(act2Objects, true);
            SetObjectsActive(act3Objects, false);
            Debug.Log($"[ActState] ✅ ACT 2: Act1 HIDDEN, Act2 objects SHOWN, Act3 HIDDEN");
        }
        // Act 3: Hide Act 1 & 2 objects, show Act 3 objects
        else if (currentAct >= 3)
        {
            SetObjectsActive(act1Objects, false);
            SetObjectsActive(act2Objects, false);
            SetObjectsActive(act3Objects, true);
            Debug.Log($"[ActState] ✅ ACT 3: Act1 & Act2 HIDDEN, Act3 objects SHOWN");
        }

        Debug.Log($"[ActState] ✅ Setup for ACT {currentAct} completed!");
    }

    void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null || objects.Length == 0)
        {
            return;
        }

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    // ✅ FUNCTION BARU - Manually switch act (optional)
    public void SwitchToAct(int act)
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetCurrentAct(act);
        }

        SetupAct(); // Re-run setup for new act
        Debug.Log($"[ActState] Manually switched to Act {act}");
    }
}
