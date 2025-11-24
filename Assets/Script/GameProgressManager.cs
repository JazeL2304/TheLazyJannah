using UnityEngine;
using TMPro;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    [Header("Progress Data")]
    public int currentAct = 1;
    public int currentDay = 1;

    [Header("UI References (Optional)")]
    public TextMeshProUGUI actText;
    public TextMeshProUGUI dayText;

    [Header("⚙️ Act-Specific UI Management")]
    public GameObject[] act1OnlyUI; // UI yang cuma muncul di Act 1
    public GameObject[] act2OnlyUI; // UI yang cuma muncul di Act 2

    [Header("⚠️ DEBUG - Reset Progress")]
    public bool resetOnStart = false; // Centang ini untuk reset ke Act 1 Day 1

    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cek apakah perlu reset
            if (resetOnStart)
            {
                ResetProgress();
                resetOnStart = false; // Auto uncheck setelah reset
                Debug.Log("[GameProgressManager] 🔄 Progress reset to ACT 1 DAY 1");
            }
            else
            {
                // Load saved progress
                LoadProgress();
            }

            Debug.Log($"[GameProgressManager] ✅ Initialized! ACT {currentAct} DAY {currentDay}");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[GameProgressManager] ⚠️ Duplicate destroyed!");
        }
    }

    void Start()
    {
        UpdateUI();
        UpdateUIForCurrentAct(); // ✅ BARU - Auto hide/show UI based on Act
    }

    // Set progress manual
    public void SetProgress(int act, int day)
    {
        currentAct = act;
        currentDay = day;
        SaveProgress();
        UpdateUI();
        UpdateUIForCurrentAct(); // ✅ Update UI visibility

        Debug.Log($"[GameProgressManager] ✅ Progress set to ACT {act} DAY {day}");
    }

    // Set current act (shortcut)
    public void SetCurrentAct(int act)
    {
        currentAct = act;
        SaveProgress();
        UpdateUI();
        UpdateUIForCurrentAct(); // ✅ Update UI visibility

        Debug.Log($"[GameProgressManager] ✅ Act set to: {act}");
    }

    // Next act otomatis
    public void NextAct()
    {
        currentAct++;
        SaveProgress();
        UpdateUI();
        UpdateUIForCurrentAct(); // ✅ Update UI visibility

        Debug.Log($"[GameProgressManager] Advanced to ACT {currentAct}");
    }

    // Add days
    public void AddDays(int days)
    {
        currentDay += days;
        SaveProgress();
        UpdateUI();

        Debug.Log($"[GameProgressManager] Added {days} days. Now DAY {currentDay}");
    }

    // ✅ BARU - Update UI visibility based on current Act
    void UpdateUIForCurrentAct()
    {
        // Hide/Show Act 1 UI
        if (act1OnlyUI != null && act1OnlyUI.Length > 0)
        {
            foreach (GameObject ui in act1OnlyUI)
            {
                if (ui != null)
                {
                    ui.SetActive(currentAct == 1);
                }
            }
            Debug.Log($"[GameProgressManager] Act 1 UI: {(currentAct == 1 ? "SHOWN" : "HIDDEN")}");
        }

        // Hide/Show Act 2 UI
        if (act2OnlyUI != null && act2OnlyUI.Length > 0)
        {
            foreach (GameObject ui in act2OnlyUI)
            {
                if (ui != null)
                {
                    ui.SetActive(currentAct == 2);
                }
            }
            Debug.Log($"[GameProgressManager] Act 2 UI: {(currentAct == 2 ? "SHOWN" : "HIDDEN")}");
        }
    }

    // Update UI text (Act & Day counter)
    void UpdateUI()
    {
        if (actText != null)
        {
            actText.text = $"ACT {currentAct}";
        }

        if (dayText != null)
        {
            dayText.text = $"DAY {currentDay}";
        }
    }

    // Save to PlayerPrefs
    void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentAct", currentAct);
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();

        Debug.Log($"[GameProgressManager] 💾 Progress saved!");
    }

    // Load from PlayerPrefs
    void LoadProgress()
    {
        currentAct = PlayerPrefs.GetInt("CurrentAct", 1);
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);

        Debug.Log($"[GameProgressManager] 📂 Loaded: ACT {currentAct} DAY {currentDay}");
    }

    // Reset progress (untuk new game)
    public void ResetProgress()
    {
        currentAct = 1;
        currentDay = 1;
        SaveProgress();
        UpdateUI();
        UpdateUIForCurrentAct(); // ✅ Update UI visibility

        Debug.Log("[GameProgressManager] 🔄 Progress reset to ACT 1 DAY 1");
    }
}
