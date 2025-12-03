using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Act3FightManager : MonoBehaviour
{
    [Header("⚔️ FIGHT UI - MINIMAL")]
    public GameObject fightUI; // Canvas fight (TRANSPARAN!)
    public TextMeshProUGUI actionText; // Text feedback aksi
    public TextMeshProUGUI controlHintText; // Hint kontrol (optional)

    [Header("🎮 FIGHT CONTROLS")]
    public KeyCode attackKey = KeyCode.Space;
    public KeyCode defendKey = KeyCode.LeftShift;
    public KeyCode dodgeKey = KeyCode.Q;

    [Header("👤 PLAYER STATS")]
    public float playerMaxHealth = 100f;
    public float playerAttackDamage = 15f;
    public float playerDefendReduction = 0.5f;
    public float dodgeChance = 0.7f;

    [Header("👹 ENEMY STATS")]
    public float enemyMaxHealth = 150f;
    public float enemyAttackDamage = 20f;
    public float enemyAttackInterval = 2f;

    [Header("⏱️ COOLDOWN SETTINGS")]
    public float attackCooldown = 1f;
    public float defendCooldown = 3f;
    public float dodgeCooldown = 4f;

    [Header("🎭 ANIMATION REFERENCES")]
    public Animator playerAnimator; // Player animator
    public Animator enemyAnimator; // Enemy animator

    [Header("🔊 SOUND EFFECTS")]
    public AudioClip attackSound;
    public AudioClip defendSound;
    public AudioClip dodgeSound;
    public AudioClip hitSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    [Header("🎬 ENDING SETTINGS")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public string nextSceneName = "Main Menu";

    [Header("📹 CAMERA SHAKE")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    // INTERNAL STATE
    private float playerCurrentHealth;
    private float enemyCurrentHealth;
    private bool isDefending = false;
    private bool isDodging = false;
    private bool fightActive = false;

    // COOLDOWN TIMERS
    private float attackCooldownTimer = 0f;
    private float defendCooldownTimer = 0f;
    private float dodgeCooldownTimer = 0f;
    private float enemyAttackTimer = 0f;

    // CAMERA
    private Camera mainCamera;
    private Vector3 originalCameraPos;

    // ANIMATION PARAMETERS (Standard Mecanim)
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_DEFEND = "Defend";
    private const string ANIM_DODGE = "Dodge";
    private const string ANIM_HIT = "Hit";
    private const string ANIM_DEATH = "Death";
    private const string ANIM_IDLE = "Idle";

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Setup camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.localPosition;
        }

        // Auto-detect animators
        if (playerAnimator == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerAnimator = player.GetComponentInChildren<Animator>();
            }
        }

        if (enemyAnimator == null)
        {
            GameObject enemy = GameObject.Find("DebtCollector"); // Adjust nama
            if (enemy != null)
            {
                enemyAnimator = enemy.GetComponentInChildren<Animator>();
            }
        }

        // Hide UI
        if (fightUI != null) fightUI.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        Debug.Log("[Act3Fight] Fight system initialized!");
    }

    void Update()
    {
        if (!fightActive) return;

        // Update cooldowns
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;
        if (defendCooldownTimer > 0) defendCooldownTimer -= Time.deltaTime;
        if (dodgeCooldownTimer > 0) dodgeCooldownTimer -= Time.deltaTime;

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

        // Reset defend state
        if (isDefending)
        {
            // Defend hanya berlaku untuk 1 attack berikutnya
            // Logic sudah handled di EnemyAttack()
        }
    }

    public void StartFight()
    {
        fightActive = true;

        // Initialize health
        playerCurrentHealth = playerMaxHealth;
        enemyCurrentHealth = enemyMaxHealth;

        // Show UI
        if (fightUI != null)
        {
            fightUI.SetActive(true);
        }

        // Show control hint
        if (controlHintText != null)
        {
            controlHintText.text = "[SPACE] Attack | [SHIFT] Defend | [Q] Dodge";
        }

        // Cursor control
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
        }

        // Set animators to idle/combat stance
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(ANIM_IDLE);
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(ANIM_IDLE);
        }

        ShowActionText("FIGHT START!", Color.yellow, 2f);

        Debug.Log("[Act3Fight] ⚔️ FIGHT STARTED!");
    }

    void PlayerAttack()
    {
        attackCooldownTimer = attackCooldown;

        // PLAY ANIMATION
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(ANIM_ATTACK);
        }

        // Deal damage
        enemyCurrentHealth -= playerAttackDamage;

        // PLAY HIT ANIMATION ON ENEMY
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(ANIM_HIT);
        }

        // Sound & effects
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        if (enableCameraShake)
        {
            StartCoroutine(CameraShake());
        }

        ShowActionText($"You ATTACK! -{playerAttackDamage} HP", Color.red, 1.5f);

        Debug.Log($"[Act3Fight] Player attacked! Enemy HP: {enemyCurrentHealth}/{enemyMaxHealth}");

        CheckFightEnd();
    }

    void PlayerDefend()
    {
        defendCooldownTimer = defendCooldown;
        isDefending = true;

        // PLAY ANIMATION
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(ANIM_DEFEND);
        }

        if (defendSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defendSound);
        }

        ShowActionText("DEFENDING! Damage reduced 50%", Color.cyan, 1.5f);

        Debug.Log("[Act3Fight] Player defending!");

        // Reset defend after next attack atau timeout
        Invoke("ResetDefend", 2f);
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
            isDodging = true;

            // PLAY ANIMATION
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(ANIM_DODGE);
            }

            if (dodgeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(dodgeSound);
            }

            ShowActionText("DODGE SUCCESS! Next attack will MISS!", Color.green, 1.5f);

            Debug.Log("[Act3Fight] Dodge successful!");

            // Set window untuk dodge next attack
            Invoke("ResetDodge", 2f);
        }
        else
        {
            // Dodge failed
            if (missSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(missSound);
            }

            ShowActionText("DODGE FAILED!", Color.gray, 1f);

            Debug.Log("[Act3Fight] Dodge failed!");
        }
    }

    void ResetDodge()
    {
        isDodging = false;
    }

    void EnemyAttack()
    {
        // PLAY ATTACK ANIMATION
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(ANIM_ATTACK);
        }

        // Check if player dodging
        if (isDodging)
        {
            ShowActionText("Enemy attack DODGED!", Color.green, 1.5f);
            isDodging = false;
            return;
        }

        float damage = enemyAttackDamage;

        // Check if player defending
        if (isDefending)
        {
            damage *= playerDefendReduction;
            ShowActionText($"Enemy attack BLOCKED! -{damage} HP", Color.cyan, 1.5f);
            isDefending = false; // Consume defend
        }
        else
        {
            ShowActionText($"Enemy HIT YOU! -{damage} HP", new Color(1f, 0.5f, 0f), 1.5f);
        }

        playerCurrentHealth -= damage;

        // PLAY HIT ANIMATION ON PLAYER
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(ANIM_HIT);
        }

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (enableCameraShake)
        {
            StartCoroutine(CameraShake());
        }

        Debug.Log($"[Act3Fight] Enemy attacked! Player HP: {playerCurrentHealth}/{playerMaxHealth}");

        CheckFightEnd();
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

        // PLAY DEATH ANIMATION ON ENEMY
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(ANIM_DEATH);
        }

        Debug.Log("[Act3Fight] 🎉 PLAYER WINS!");

        if (fightUI != null)
        {
            fightUI.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        ShowActionText("VICTORY! You survived!", Color.green, 5f);

        Invoke("LoadNextScene", 5f);
    }

    void Defeat()
    {
        fightActive = false;

        // PLAY DEATH ANIMATION ON PLAYER
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(ANIM_DEATH);
        }

        Debug.Log("[Act3Fight] 💀 PLAYER DEFEATED!");

        if (fightUI != null)
        {
            fightUI.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        ShowActionText("DEFEATED... You were taken away...", Color.red, 5f);

        Invoke("RestartFight", 5f);
    }

    void LoadNextScene()
    {
        Debug.Log("[Act3Fight] Loading: " + nextSceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    void RestartFight()
    {
        Debug.Log("[Act3Fight] Restarting fight...");

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        StartFight();
    }

    void ShowActionText(string message, Color color, float duration)
    {
        if (actionText != null)
        {
            actionText.text = message;
            actionText.color = color;

            StopCoroutine("FadeActionText");
            StartCoroutine(FadeActionText(duration));
        }

        Debug.Log("[Act3Fight] " + message);
    }

    IEnumerator FadeActionText(float duration)
    {
        yield return new WaitForSeconds(duration);

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

    public void ManualStartFight()
    {
        StartFight();
    }
}