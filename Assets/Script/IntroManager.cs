using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("🌑 BLACK SCREEN FADE")]
    public CanvasGroup blackScreenFade; // Panel hitam untuk fade in/out
    public float blackFadeInDuration = 1f; // Durasi fade dari hitam ke transparent
    public float blackFadeOutDuration = 1f; // Durasi fade ke hitam di akhir

    [Header("📝 TEXT ELEMENTS")]
    public CanvasGroup loadingText;
    public CanvasGroup actText;
    public CanvasGroup dayText;

    [Header("✏️ TEXT CONTENT")]
    public TextMeshProUGUI actTextContent;
    public TextMeshProUGUI dayTextContent;

    [Header("⏱️ TIMING SETTINGS")]
    public string nextScene = "Enviroment Game";
    public float fadeDuration = 1f;
    public float stayDuration = 1.5f;

    [Header("🎵 AUDIO (Optional)")]
    public AudioClip transitionSound;
    private AudioSource audioSource;

    private void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // ✅ BLACK SCREEN MULAI DARI HITAM PENUH!
        if (blackScreenFade != null)
        {
            blackScreenFade.alpha = 1f; // Start fully black
            blackScreenFade.gameObject.SetActive(true);
        }

        // Pastikan semua teks mulai dengan transparan
        if (loadingText != null) loadingText.alpha = 0;
        if (actText != null) actText.alpha = 0;
        if (dayText != null) dayText.alpha = 0;

        // UPDATE TEXT BERDASARKAN GAME PROGRESS
        UpdateActDayText();

        StartCoroutine(PlayIntro());
    }

    void UpdateActDayText()
    {
        if (GameProgressManager.Instance != null)
        {
            int act = GameProgressManager.Instance.currentAct;
            int day = GameProgressManager.Instance.currentDay;

            // Update ACT text
            if (actTextContent != null)
            {
                actTextContent.text = $"ACT {act}";
            }

            // Update DAY text
            if (dayTextContent != null)
            {
                dayTextContent.text = $"DAY {day}";
            }

            Debug.Log($"[IntroManager] Displaying ACT {act} DAY {day}");
        }
        else
        {
            Debug.LogWarning("[IntroManager] GameProgressManager not found! Using default values.");

            if (actTextContent != null)
                actTextContent.text = "ACT 1";

            if (dayTextContent != null)
                dayTextContent.text = "DAY 1";
        }
    }

    IEnumerator PlayIntro()
    {
        // Play transition sound
        if (transitionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // =========================================================================
        // 🌑 FADE IN DARI BLACK (Layar hitam → Loading text muncul)
        // =========================================================================
        yield return StartCoroutine(FadeBlackScreen(false, blackFadeInDuration));

        // =========================================================================
        // 1. LOADING... (Total 7.0 detik)
        // =========================================================================
        float totalLoadingTime = 7.0f;

        // 1.1. Fade In Loading Text
        if (loadingText != null)
        {
            yield return StartCoroutine(FadeText(loadingText, true, fadeDuration));
        }

        // 1.2. Tahan
        yield return new WaitForSeconds(totalLoadingTime - (2 * fadeDuration));

        // 1.3. Fade Out Loading Text
        if (loadingText != null)
        {
            yield return StartCoroutine(FadeText(loadingText, false, fadeDuration));
        }

        // =========================================================================
        // 2. ACT (Total 5.0 detik)
        // =========================================================================
        float totalActTime = 5.0f;

        if (actText != null)
        {
            // 2.1. Fade In
            yield return StartCoroutine(FadeText(actText, true, fadeDuration));

            // 2.2. Tahan
            yield return new WaitForSeconds(totalActTime - (2 * fadeDuration));

            // 2.3. Fade Out
            yield return StartCoroutine(FadeText(actText, false, fadeDuration));
        }

        // =========================================================================
        // 3. DAY (Total 5.0 detik)
        // =========================================================================
        float totalDayTime = 5.0f;

        if (dayText != null)
        {
            // 3.1. Fade In
            yield return StartCoroutine(FadeText(dayText, true, fadeDuration));

            // 3.2. Tahan
            yield return new WaitForSeconds(totalDayTime - fadeDuration);
        }

        // =========================================================================
        // 🌑 FADE OUT KE BLACK (Day text → Hitam penuh)
        // =========================================================================
        yield return StartCoroutine(FadeBlackScreen(true, blackFadeOutDuration));

        // =========================================================================
        // 4. LOAD SCENE
        // =========================================================================
        Debug.Log($"[IntroManager] Loading scene: {nextScene}");
        SceneManager.LoadScene(nextScene);
    }

    // Fade CanvasGroup text (Loading/Act/Day)
    IEnumerator FadeText(CanvasGroup cg, bool fadeIn, float duration)
    {
        float start = fadeIn ? 0 : 1;
        float end = fadeIn ? 1 : 0;
        float time = 0;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = end;
    }

    // ✅ BARU - Fade Black Screen In/Out
    IEnumerator FadeBlackScreen(bool fadeToBlack, float duration)
    {
        if (blackScreenFade == null)
        {
            Debug.LogWarning("[IntroManager] Black Screen Fade not assigned!");
            yield break;
        }

        float start = fadeToBlack ? 0f : 1f;
        float end = fadeToBlack ? 1f : 0f;
        float time = 0;

        while (time < duration)
        {
            blackScreenFade.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackScreenFade.alpha = end;

        Debug.Log($"[IntroManager] Black screen fade {(fadeToBlack ? "OUT" : "IN")} complete!");
    }
}