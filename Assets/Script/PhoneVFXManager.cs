using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// VFX Manager untuk Phone scene (Act 2 - Tampilan CC)
/// AUTO-START: Effects langsung jalan begitu Phone scene load!
/// CUMA AKTIF DI PHONE SCENE AJA - ga akan jalan di scene lain!
/// </summary>
public class PhoneVFXManager : MonoBehaviour
{
    [Header("🎯 VFX TARGETS")]
    [Tooltip("Canvas atau parent object dari phone UI")]
    public GameObject phoneCanvas;

    [Tooltip("Image untuk screen flash effect (auto-create jika kosong)")]
    public Image flashOverlay;

    [Tooltip("Camera untuk shake effect (auto-detect Main Camera)")]
    public Camera phoneCamera;

    [Header("🎬 AUTO-START SETTINGS")]
    [Tooltip("Auto-start effects when Phone scene loads")]
    public bool autoStartEffects = true;

    [Tooltip("Delay before auto-start (seconds)")]
    public float autoStartDelay = 0.5f;

    [Tooltip("Auto-trigger intense effect on start (shake + flash sekali)")]
    public bool autoTriggerIntenseEffect = false;

    [Header("🔴 GLITCH EFFECT")]
    [Tooltip("Enable glitch effect (UI shake random)")]
    public bool enableGlitch = true;

    [Tooltip("Glitch intensity (0-1) | Recommended: 0.3-0.5")]
    [Range(0f, 1f)]
    public float glitchIntensity = 0.3f;

    [Tooltip("Glitch interval in seconds | Recommended: 1.5-2.5")]
    public float glitchInterval = 2f;

    [Header("⚠️ WARNING PULSE")]
    [Tooltip("Enable warning pulse (red overlay pulse)")]
    public bool enableWarningPulse = true;

    [Tooltip("Warning color (red with alpha)")]
    public Color warningColor = new Color(1f, 0f, 0f, 0.3f);

    [Tooltip("Pulse speed | Recommended: 2-3")]
    public float pulseSpeed = 2f;

    [Header("📳 SCREEN SHAKE")]
    [Tooltip("Enable screen shake effect")]
    public bool enableScreenShake = true;

    [Tooltip("Shake intensity | Recommended: 0.1-0.2")]
    public float shakeIntensity = 0.1f;

    [Tooltip("Shake duration in seconds")]
    public float shakeDuration = 0.3f;

    [Header("🔥 RED FLASH")]
    [Tooltip("Red flash intensity (0-1)")]
    [Range(0f, 1f)]
    public float redFlashIntensity = 0.5f;

    [Tooltip("Flash duration in seconds")]
    public float flashDuration = 0.2f;

    [Header("💀 DARK VIGNETTE")]
    [Tooltip("Enable dark vignette (hopeless/trapped feeling)")]
    public bool enableDarkVignette = false;

    [Tooltip("Vignette darkness (0-1) | 0.3-0.5 recommended")]
    [Range(0f, 1f)]
    public float vignetteDarkness = 0.5f;

    [Header("🔒 SCENE SAFETY")]
    [Tooltip("Force VFX only in Phone scene (safety check)")]
    public bool onlyInPhoneScene = true;

    [Tooltip("Phone scene name (must match exactly!)")]
    public string phoneSceneName = "Phone";

    // INTERNAL STATE
    private Vector3 originalCameraPos;
    private bool isGlitching = false;
    private bool isPulsing = false;
    private Coroutine glitchCoroutine;
    private Coroutine pulseCoroutine;
    private bool isInitialized = false;

    void Start()
    {
        // 🔒 SAFETY CHECK - Cuma jalan di Phone scene!
        if (onlyInPhoneScene)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != phoneSceneName)
            {
                Debug.LogWarning($"[PhoneVFX] Not in Phone scene! Current: {currentScene}. VFX disabled.");
                this.enabled = false;
                return;
            }
        }

        // Auto-detect phone camera jika tidak di-assign
        if (phoneCamera == null)
        {
            phoneCamera = Camera.main;
            if (phoneCamera != null)
            {
                Debug.Log("[PhoneVFX] Camera auto-detected: " + phoneCamera.name);
            }
        }

        // Save original camera position untuk shake reset
        if (phoneCamera != null)
        {
            originalCameraPos = phoneCamera.transform.localPosition;
        }

        // Auto-create flash overlay jika tidak ada
        if (flashOverlay == null)
        {
            CreateFlashOverlay();
        }

        isInitialized = true;

        // ✅ AUTO-START EFFECTS!
        if (autoStartEffects)
        {
            StartCoroutine(AutoStartEffectsSequence());
        }
        else
        {
            Debug.Log("[PhoneVFX] VFX Manager initialized - Manual mode (auto-start disabled)");
        }
    }

    IEnumerator AutoStartEffectsSequence()
    {
        yield return new WaitForSeconds(autoStartDelay);

        Debug.Log("[PhoneVFX] 🎬 AUTO-STARTING VFX EFFECTS!");

        // Start continuous effects
        if (enableGlitch)
        {
            StartGlitch();
            Debug.Log("[PhoneVFX] ✅ Glitch effect started");
        }

        if (enableWarningPulse)
        {
            StartWarningPulse();
            Debug.Log("[PhoneVFX] ✅ Warning pulse started");
        }

        if (enableDarkVignette)
        {
            ApplyDarkVignette();
            Debug.Log("[PhoneVFX] ✅ Dark vignette applied");
        }

        // Trigger one-time intense effect (optional)
        if (autoTriggerIntenseEffect && enableScreenShake)
        {
            yield return new WaitForSeconds(0.3f);
            TriggerIntenseEffect();
            Debug.Log("[PhoneVFX] ✅ Intense effect triggered!");
        }

        Debug.Log("[PhoneVFX] All VFX effects are now active!");
    }

    void OnDestroy()
    {
        // Cleanup when object destroyed
        StopAllEffects();
    }

    void OnDisable()
    {
        // Stop all effects when disabled
        if (isInitialized)
        {
            StopAllEffects();
        }
    }

    // ========================================
    // 🔴 GLITCH EFFECT
    // ========================================

    /// <summary>
    /// Start continuous glitch effect (UI shake loop)
    /// </summary>
    public void StartGlitch()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }
        glitchCoroutine = StartCoroutine(GlitchLoop());
    }

    /// <summary>
    /// Stop glitch effect
    /// </summary>
    public void StopGlitch()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }
        isGlitching = false;
    }

    /// <summary>
    /// Trigger single glitch effect (one-time)
    /// </summary>
    public void TriggerGlitch()
    {
        StartCoroutine(GlitchEffect());
    }

    IEnumerator GlitchLoop()
    {
        isGlitching = true;

        while (isGlitching)
        {
            yield return new WaitForSeconds(glitchInterval);
            StartCoroutine(GlitchEffect());
        }
    }

    IEnumerator GlitchEffect()
    {
        if (phoneCanvas == null) yield break;

        RectTransform rect = phoneCanvas.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 originalPos = rect.localPosition;
        float duration = Random.Range(0.05f, 0.15f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * glitchIntensity * 20f;
            float offsetY = Random.Range(-1f, 1f) * glitchIntensity * 20f;

            rect.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localPosition = originalPos;
    }

    // ========================================
    // ⚠️ WARNING PULSE
    // ========================================

    /// <summary>
    /// Start continuous warning pulse (red overlay fade in/out)
    /// </summary>
    public void StartWarningPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(WarningPulseLoop());
    }

    /// <summary>
    /// Stop warning pulse
    /// </summary>
    public void StopWarningPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        isPulsing = false;

        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    IEnumerator WarningPulseLoop()
    {
        isPulsing = true;

        while (isPulsing)
        {
            // Fade in
            float alpha = 0f;
            while (alpha < warningColor.a)
            {
                alpha += Time.deltaTime * pulseSpeed;
                if (flashOverlay != null)
                {
                    Color c = warningColor;
                    c.a = alpha;
                    flashOverlay.color = c;
                }
                yield return null;
            }

            // Fade out
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * pulseSpeed;
                if (flashOverlay != null)
                {
                    Color c = warningColor;
                    c.a = alpha;
                    flashOverlay.color = c;
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // ========================================
    // 📳 SCREEN SHAKE
    // ========================================

    /// <summary>
    /// Trigger screen shake effect (one-time)
    /// </summary>
    public void TriggerScreenShake()
    {
        if (enableScreenShake)
        {
            StartCoroutine(ScreenShakeEffect());
        }
    }

    IEnumerator ScreenShakeEffect()
    {
        if (phoneCamera == null) yield break;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            phoneCamera.transform.localPosition = originalCameraPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        phoneCamera.transform.localPosition = originalCameraPos;
    }

    // ========================================
    // 🔥 RED FLASH
    // ========================================

    /// <summary>
    /// Trigger red flash effect (one-time shock effect)
    /// </summary>
    public void TriggerRedFlash()
    {
        StartCoroutine(RedFlashEffect());
    }

    IEnumerator RedFlashEffect()
    {
        if (flashOverlay == null) yield break;

        Color red = new Color(1f, 0f, 0f, redFlashIntensity);

        // Flash in
        float elapsed = 0f;
        while (elapsed < flashDuration / 2f)
        {
            float alpha = Mathf.Lerp(0, redFlashIntensity, elapsed / (flashDuration / 2f));
            flashOverlay.color = new Color(1f, 0f, 0f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Flash out
        elapsed = 0f;
        while (elapsed < flashDuration / 2f)
        {
            float alpha = Mathf.Lerp(redFlashIntensity, 0, elapsed / (flashDuration / 2f));
            flashOverlay.color = new Color(1f, 0f, 0f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    // ========================================
    // 💀 DARK VIGNETTE
    // ========================================

    /// <summary>
    /// Apply dark vignette (hopeless/trapped feeling)
    /// </summary>
    public void ApplyDarkVignette()
    {
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(0f, 0f, 0f, vignetteDarkness);
            Debug.Log("[PhoneVFX] Dark vignette applied");
        }
    }

    /// <summary>
    /// Remove dark vignette
    /// </summary>
    public void RemoveDarkVignette()
    {
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(0f, 0f, 0f, 0f);
            Debug.Log("[PhoneVFX] Dark vignette removed");
        }
    }

    // ========================================
    // 🎨 HELPER: CREATE FLASH OVERLAY
    // ========================================

    void CreateFlashOverlay()
    {
        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[PhoneVFX] Canvas not found! Flash overlay cannot be created.");
            return;
        }

        // Create overlay GameObject
        GameObject overlayObj = new GameObject("FlashOverlay_VFX");
        overlayObj.transform.SetParent(canvas.transform, false);

        // Add Image component
        flashOverlay = overlayObj.AddComponent<Image>();
        flashOverlay.color = new Color(1f, 1f, 1f, 0f);

        // Make it full screen
        RectTransform rect = overlayObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        // Set as last sibling (render on top of everything)
        overlayObj.transform.SetAsLastSibling();

        // Disable raycast (tidak block UI input)
        flashOverlay.raycastTarget = false;

        Debug.Log("[PhoneVFX] Flash overlay created automatically!");
    }

    // ========================================
    // 🎮 PUBLIC API - UNTUK SCRIPT LAIN
    // ========================================

    /// <summary>
    /// Trigger combo effect: Shake + Red Flash + Glitch (untuk momen intense!)
    /// </summary>
    public void TriggerIntenseEffect()
    {
        TriggerScreenShake();
        TriggerRedFlash();
        TriggerGlitch();
        Debug.Log("[PhoneVFX] 💥 INTENSE EFFECT TRIGGERED!");
    }

    /// <summary>
    /// Start danger mode: Pulse + Glitch continuous
    /// </summary>
    public void StartDangerMode()
    {
        StartWarningPulse();
        StartGlitch();
        Debug.Log("[PhoneVFX] ⚠️ DANGER MODE STARTED!");
    }

    /// <summary>
    /// Stop all VFX effects
    /// </summary>
    public void StopAllEffects()
    {
        StopGlitch();
        StopWarningPulse();
        RemoveDarkVignette();
        Debug.Log("[PhoneVFX] All effects stopped.");
    }

    /// <summary>
    /// Check if VFX is currently active
    /// </summary>
    public bool IsEffectActive()
    {
        return isGlitching || isPulsing;
    }

    /// <summary>
    /// Set glitch intensity dynamically
    /// </summary>
    public void SetGlitchIntensity(float intensity)
    {
        glitchIntensity = Mathf.Clamp01(intensity);
        Debug.Log($"[PhoneVFX] Glitch intensity set to {glitchIntensity}");
    }

    /// <summary>
    /// Set pulse speed dynamically
    /// </summary>
    public void SetPulseSpeed(float speed)
    {
        pulseSpeed = Mathf.Max(0.5f, speed);
        Debug.Log($"[PhoneVFX] Pulse speed set to {pulseSpeed}");
    }
}