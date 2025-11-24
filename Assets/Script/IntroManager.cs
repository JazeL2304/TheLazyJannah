using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    // Menggunakan CanvasGroup untuk kontrol alpha/fading
    public CanvasGroup loadingText;
    public CanvasGroup actText;
    public CanvasGroup dayText;

    // TextMeshPro untuk update text dinamis
    public TextMeshProUGUI actTextContent;   // Text "ACT 1" atau "ACT 2"
    public TextMeshProUGUI dayTextContent;   // Text "DAY 1" atau "DAY 30"

    // Sesuaikan nama scene tujuan di Inspector
    public string nextScene = "Enviroment Game";

    // Atur durasi fade di Inspector (misalnya 1.0f)
    public float fadeDuration = 1f;

    // Atur durasi teks diam sebelum fade out (misalnya 1.5f)
    public float stayDuration = 1.5f;

    private void Start()
    {
        // Pastikan semua teks mulai dengan transparan
        loadingText.alpha = 0;
        actText.alpha = 0;
        dayText.alpha = 0;

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

            // Update ACT text - SIMPLE FORMAT!
            if (actTextContent != null)
            {
                actTextContent.text = $"ACT {act}"; // ← Udah ada spasi
            }

            // Update DAY text - TAMBAH SPASI!
            if (dayTextContent != null)
            {
                dayTextContent.text = $"DAY{day}"; // ← TAMBAH SPASI DI SINI!
            }

            Debug.Log($"[IntroManager] Displaying ACT {act} DAY {day}");
        }
        else
        {
            Debug.LogWarning("[IntroManager] GameProgressManager not found! Using default values.");

            // Default values
            if (actTextContent != null)
                actTextContent.text = "ACT 1";

            if (dayTextContent != null)
                dayTextContent.text = "DAY 1"; // ← Dan di sini juga!
        }
    }


    IEnumerator PlayIntro()
    {
        float totalLoadingTime = 7.0f; // Target waktu Loading
        float totalActTime = 5.0f;     // Target waktu Act 1
        float totalDayTime = 5.0f;     // Target waktu Day 1

        // =========================================================================
        // 1. LOADING... (Total 7.0 detik)
        // =========================================================================
        // 1.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(loadingText, true, fadeDuration));

        // 1.2. Tahan (5.0s)
        yield return new WaitForSeconds(totalLoadingTime - (2 * fadeDuration));

        // 1.3. Fade Out (1.0s)
        yield return StartCoroutine(FadeText(loadingText, false, fadeDuration));

        // =========================================================================
        // 2. ACT (Total 5.0 detik)
        // =========================================================================
        // 2.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(actText, true, fadeDuration));

        // 2.2. Tahan (3.0s)
        yield return new WaitForSeconds(totalActTime - (2 * fadeDuration));

        // 2.3. Fade Out (1.0s)
        yield return StartCoroutine(FadeText(actText, false, fadeDuration));

        // =========================================================================
        // 3. DAY (Total 5.0 detik)
        // =========================================================================
        // 3.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(dayText, true, fadeDuration));

        // 3.2. Tahan (4.0s)
        yield return new WaitForSeconds(totalDayTime - fadeDuration);

        // Pindah ke scene utama
        SceneManager.LoadScene(nextScene);
    }

    // Fungsi Fade Dasar
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
}
