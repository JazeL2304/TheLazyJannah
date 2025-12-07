using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// STEALTH MANAGER - ACT 1 ONLY
/// ⚠️ HANYA AKTIF SETELAH QUEST DIMULAI! BUKAN AUTO-START!
/// </summary>
public class StealthManager : MonoBehaviour
{
    [Header("🎮 ACT & MISSION VALIDATION")]
    [Tooltip("Only active in Act 1")]
    public bool onlyInAct1 = true;

    [Tooltip("CRITICAL: Stealth manager starts DISABLED! Activated by GameManager.")]
    public bool startDisabled = true;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI restartText;

    [Header("Checkpoint")]
    public Transform currentCheckpoint;

    [Header("Detection Settings")]
    public float detectionCooldown = 2f;

    [Header("🔊 Audio (Optional)")]
    public AudioClip detectionSound;
    private AudioSource audioSource;

    // INTERNAL STATE
    private bool isDetected = false;
    private GameObject player;
    private float lastDetectionTime = 0f;
    private bool missionActive = false; // ← BARU! Control flag

    void Awake()
    {
        // ✅ ENSURE GAMEOBJECT IS ACTIVE!
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[StealthManager] GameObject was inactive - activating!");
        }

        // ✅ HIDE GAME OVER PANEL IMMEDIATELY!
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("[StealthManager] Game Over Panel FORCE HIDDEN at Awake!");
        }

        // ✅ START DISABLED JIKA SETTING ENABLED!
        if (startDisabled)
        {
            this.enabled = false;
            Debug.Log("[StealthManager] ⏸️ Script DISABLED - GameObject ACTIVE - waiting for mission!");
        }
    }

    void Start()
    {
        // ✅ VALIDASI ACT - HANYA AKTIF DI ACT 1!
        if (onlyInAct1 && GameProgressManager.Instance != null)
        {
            int currentAct = GameProgressManager.Instance.currentAct;

            if (currentAct != 1)
            {
                Debug.Log($"[StealthManager] Not Act 1 (Current: Act {currentAct}) - Destroying script!");

                // FORCE HIDE PANEL!
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(false);
                }

                Destroy(this);
                return;
            }
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[StealthManager] ❌ Player not found! Tag player as 'Player'!");
        }

        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Debug.Log("[StealthManager] ✅ Initialized - Mission active: " + missionActive);
    }

    void Update()
    {
        // ⚠️ JANGAN DETECT JIKA MISSION BELUM AKTIF!
        if (!missionActive)
        {
            return;
        }

        // Check for restart input when detected
        if (isDetected && Input.GetKeyDown(KeyCode.R))
        {
            RestartFromCheckpoint();
        }
    }

    /// <summary>
    /// ✅ BARU! Aktivasi stealth mission (dipanggil dari GameManager)
    /// </summary>
    public void ActivateMission()
    {
        missionActive = true;
        this.enabled = true;
        Debug.Log("[StealthManager] 🎯 MISSION ACTIVATED! Detection is now LIVE!");
    }

    /// <summary>
    /// Deactivate mission (untuk testing atau cancel)
    /// </summary>
    public void DeactivateMission()
    {
        missionActive = false;
        Debug.Log("[StealthManager] Mission deactivated - detection paused");
    }

    /// <summary>
    /// Called by NPCVisionCone when player is detected
    /// </summary>
    public void OnPlayerDetected(string detectorName)
    {
        // ⚠️ IGNORE DETECTION JIKA MISSION BELUM AKTIF!
        if (!missionActive)
        {
            Debug.Log($"[StealthManager] Detection ignored - mission not active yet!");
            return;
        }

        // Cooldown check
        if (Time.time - lastDetectionTime < detectionCooldown)
        {
            return;
        }

        if (isDetected)
        {
            return;
        }

        isDetected = true;
        lastDetectionTime = Time.time;

        Debug.Log($"[StealthManager] 🚨 PLAYER DETECTED BY: {detectorName}");

        // Play detection sound
        if (audioSource != null && detectionSound != null)
        {
            audioSource.PlayOneShot(detectionSound);
        }

        StartCoroutine(ShowGameOver(detectorName));
    }

    IEnumerator ShowGameOver(string detectorName)
    {
        // Freeze player movement
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text = "TERTANGKAP!";
            }

            if (restartText != null)
            {
                restartText.text = "Tekan [R] untuk restart";
            }
        }
        else
        {
            Debug.LogError("[StealthManager] ❌ Game Over Panel is NULL!");
        }

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;
    }

    void RestartFromCheckpoint()
    {
        Debug.Log("[StealthManager] 🔄 Restarting from checkpoint...");

        if (currentCheckpoint != null && player != null)
        {
            // Teleport player
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = currentCheckpoint.position;
                player.transform.rotation = currentCheckpoint.rotation;
                cc.enabled = true;
            }

            // Re-enable player controller
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
        else
        {
            // Reload scene if no checkpoint
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset state
        isDetected = false;
        lastDetectionTime = Time.time;

        Debug.Log("[StealthManager] ✅ Restart complete!");
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
        Debug.Log($"[StealthManager] ✅ Checkpoint set: {checkpoint.name}");
    }

    public bool IsDetected()
    {
        return isDetected;
    }

    public bool IsMissionActive()
    {
        return missionActive;
    }

    void OnDestroy()
    {
        // Cleanup
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}