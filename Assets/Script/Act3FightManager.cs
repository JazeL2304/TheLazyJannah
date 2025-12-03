using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Act3FightManager : MonoBehaviour
{
    [Header("⚔️ FIGHT UI")]
    public GameObject fightUI; // Panel fight UI
    public Image playerHealthBar;
    public Image enemyHealthBar;
    public TextMeshProUGUI playerHealthText; // "HP: 100/100"
    public TextMeshProUGUI enemyHealthText; // "HP: 150/150"
    public TextMeshProUGUI actionText; // Text untuk feedback aksi

    [Header("🎮 FIGHT CONTROLS")]
    public KeyCode attackKey = KeyCode.Space;
    public KeyCode defendKey = KeyCode.LeftShift;
    public KeyCode dodgeKey = KeyCode.Q;

    [Header("👤 PLAYER STATS")]
    public float playerMaxHealth = 100f;
    public float playerAttackDamage = 15f;
    public float playerDefendReduction = 0.5f; // 50% damage reduction saat defend
    public float dodgeChance = 0.7f; // 70% chance dodge berhasil

    [Header("👹 ENEMY STATS")]
    public float enemyMaxHealth = 150f;
    public float enemyAttackDamage = 20f;
    public float enemyAttackInterval = 2f; // Musuh menyerang tiap 2 detik

    [Header("⏱️ COOLDOWN SETTINGS")]
    public float attackCooldown = 1f;
    public float defendCooldown = 3f;
    public float dodgeCooldown = 4f;

    [Header("📊 UI COOLDOWN INDICATORS")]
    public Image attackCooldownFill;
    public Image defendCooldownFill;
    public Image dodgeCooldownFill;
    public TextMeshProUGUI attackKeyText;
    public TextMeshProUGUI defendKeyText;
    public TextMeshProUGUI dodgeKeyText;

    [Header("🔊 SOUND EFFECTS")]
    public AudioClip attackSound;
    public AudioClip defendSound;
    public AudioClip dodgeSound;
    public AudioClip hitSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    [Header("🎬 ENDING SETTINGS")]
    public GameObject victoryPanel; // Panel kalo player menang
    public GameObject defeatPanel; // Panel kalo player kalah
    public string nextSceneName = "Main Menu"; // Scene setelah fight

    [Header("📹 CAMERA SHAKE")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    private float playerCurrentHealth;
    private float enemyCurrentHealth;
    private bool isDefending = false;
    private bool fightActive = false;

    // Cooldown timers
    private float attackCooldownTimer = 0f;
    private float defendCooldownTimer = 0f;
    private float dodgeCooldownTimer = 0f;
    private float enemyAttackTimer = 0f;

    private Camera mainCamera;
    private Vector3 originalCameraPos;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.localPosition;
        }

        // Hide all UI at start
        if (fightUI != null) fightUI.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        // Setup key text labels
        if (attackKeyText != null) attackKeyText.text = $"[{attackKey}] ATTACK";
        if (defendKeyText != null) defendKeyText.text = $"[{defendKey}] DEFEND";
        if (dodgeKeyText != null) dodgeKeyText.text = $"[{dodgeKey}] DODGE";

        Debug.Log("[Act3Fight] Fight system initialized!");
    }

    void Update()
    {
        if (!fightActive) return;

        // Update cooldown timers
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownFill != null)
                attackCooldownFill.fillAmount = attackCooldownTimer / attackCooldown;
        }

        if (defendCooldownTimer > 0)
        {
            defendCooldownTimer -= Time.deltaTime;
            if (defendCooldownFill != null)
                defendCooldownFill.fillAmount = defendCooldownTimer / defendCooldown;
        }

        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
            if (dodgeCooldownFill != null)
                dodgeCooldownFill.fillAmount = dodgeCooldownTimer / dodgeCooldown;
        }

        // Player input
        if (Input.GetKeyDown(attackKey) && attackCooldownTimer <= 0)
        {
            PlayerAttack();
        }

        if (Input.GetKeyDown(defendKey) && defendCooldownTimer <= 0)
        {
            PlayerDefend();
        }

        if (Input.GetKeyDown(dodgeKey) && dodgeCooldownTimer <= 0)
        {
            PlayerDodge();
        }

        // Enemy attack timer
        enemyAttackTimer += Time.deltaTime;
        if (enemyAttackTimer >= enemyAttackInterval)
        {
            EnemyAttack();
            enemyAttackTimer = 0f;
        }
    }

    public void StartFight()
    {
        fightActive = true;

        // Initialize health
        playerCurrentHealth = playerMaxHealth;
        enemyCurrentHealth = enemyMaxHealth;

        // Show fight UI
        if (fightUI != null)
        {
            fightUI.SetActive(true);
        }

        UpdateHealthUI();

        // Enable player control (cursor visible untuk UI)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        ShowActionText("FIGHT START! Defend yourself!", Color.yellow);

        Debug.Log("[Act3Fight] ⚔️ FIGHT STARTED!");
    }

    void PlayerAttack()
    {
        attackCooldownTimer = attackCooldown;

        // Deal damage to enemy
        enemyCurrentHealth -= playerAttackDamage;

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        if (enableCameraShake)
        {
            StartCoroutine(CameraShake());
        }

        ShowActionText($"You ATTACK! Dealt {playerAttackDamage} damage!", Color.red);

        Debug.Log($"[Act3Fight] Player attacked! Enemy HP: {enemyCurrentHealth}/{enemyMaxHealth}");

        UpdateHealthUI();
        CheckFightEnd();
    }

    void PlayerDefend()
    {
        defendCooldownTimer = defendCooldown;
        isDefending = true;

        if (defendSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defendSound);
        }

        ShowActionText("DEFENDING! Damage reduced!", Color.blue);

        Debug.Log("[Act3Fight] Player defending!");

        // Reset defend after 1 second
        Invoke("ResetDefend", 1f);
    }

    void ResetDefend()
    {
        isDefending = false;
    }

    void PlayerDodge()
    {
        dodgeCooldownTimer = dodgeCooldown;

        float random = Random.Range(0f, 1f);

        if (random <= dodgeChance)
        {
            // Dodge successful
            if (dodgeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(dodgeSound);
            }

            ShowActionText("DODGE SUCCESS! Next attack will miss!", Color.green);

            Debug.Log("[Act3Fight] Dodge successful!");

            // Set flag untuk skip next enemy attack
            StartCoroutine(DodgeWindow());
        }
        else
        {
            // Dodge failed
            if (missSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(missSound);
            }

            ShowActionText("DODGE FAILED!", Color.gray);

            Debug.Log("[Act3Fight] Dodge failed!");
        }
    }

    IEnumerator DodgeWindow()
    {
        bool dodging = true;

        // Window untuk dodge (1 detik)
        float timer = 0f;
        while (timer < 1f)
        {
            if (enemyAttackTimer >= enemyAttackInterval && dodging)
            {
                // Skip enemy attack
                enemyAttackTimer = 0f;
                dodging = false;

                ShowActionText("Enemy attack DODGED!", Color.green);
                Debug.Log("[Act3Fight] Enemy attack dodged!");
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    void EnemyAttack()
    {
        float damage = enemyAttackDamage;

        // Check if player defending
        if (isDefending)
        {
            damage *= playerDefendReduction;
            ShowActionText($"Enemy attacked! Blocked {(int)(enemyAttackDamage * (1 - playerDefendReduction))} damage!", Color.cyan);
        }
        else
        {
            ShowActionText($"Enemy HIT YOU for {damage} damage!", new Color(1f, 0.5f, 0f)); // Orange
        }

        playerCurrentHealth -= damage;

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (enableCameraShake)
        {
            StartCoroutine(CameraShake());
        }

        Debug.Log($"[Act3Fight] Enemy attacked! Player HP: {playerCurrentHealth}/{playerMaxHealth}");

        UpdateHealthUI();
        CheckFightEnd();
    }

    void UpdateHealthUI()
    {
        // Player health bar
        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = playerCurrentHealth / playerMaxHealth;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"HP: {(int)playerCurrentHealth}/{(int)playerMaxHealth}";
        }

        // Enemy health bar
        if (enemyHealthBar != null)
        {
            enemyHealthBar.fillAmount = enemyCurrentHealth / enemyMaxHealth;
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"HP: {(int)enemyCurrentHealth}/{(int)enemyMaxHealth}";
        }
    }

    void CheckFightEnd()
    {
        if (enemyCurrentHealth <= 0)
        {
            Victory();
        }
        else if (playerCurrentHealth <= 0)
        {
            Defeat();
        }
    }

    void Victory()
    {
        fightActive = false;

        Debug.Log("[Act3Fight] 🎉 PLAYER WINS!");

        if (fightUI != null)
        {
            fightUI.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        ShowActionText("VICTORY! You defeated the debt collector!", Color.green);

        // Auto load next scene after 5 seconds
        Invoke("LoadNextScene", 5f);
    }

    void Defeat()
    {
        fightActive = false;

        Debug.Log("[Act3Fight] 💀 PLAYER DEFEATED!");

        if (fightUI != null)
        {
            fightUI.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        ShowActionText("DEFEATED... You were taken away...", Color.red);

        // Auto restart or load game over scene
        Invoke("RestartFight", 5f);
    }

    void LoadNextScene()
    {
        Debug.Log("[Act3Fight] Loading next scene: " + nextSceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    void RestartFight()
    {
        Debug.Log("[Act3Fight] Restarting fight...");

        // Hide defeat panel
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        // Restart fight
        StartFight();
    }

    void ShowActionText(string message, Color color)
    {
        if (actionText != null)
        {
            actionText.text = message;
            actionText.color = color;

            // Fade out after 2 seconds
            StopCoroutine("FadeActionText");
            StartCoroutine(FadeActionText());
        }

        Debug.Log("[Act3Fight] " + message);
    }

    IEnumerator FadeActionText()
    {
        yield return new WaitForSeconds(2f);

        if (actionText != null)
        {
            float alpha = 1f;

            while (alpha > 0)
            {
                alpha -= Time.deltaTime * 2f;
                Color c = actionText.color;
                c.a = alpha;
                actionText.color = c;
                yield return null;
            }
        }
    }

    IEnumerator CameraShake()
    {
        if (mainCamera == null) yield break;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            mainCamera.transform.localPosition = originalCameraPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCameraPos;
    }

    // Manual trigger untuk testing
    public void ManualStartFight()
    {
        StartFight();
    }
}