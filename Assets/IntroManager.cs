using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Tetap diperlukan jika objek UI adalah TextMeshPro
using System.Collections;

public class IntroManager : MonoBehaviour
{
    // Menggunakan CanvasGroup untuk kontrol alpha/fading
    public CanvasGroup loadingText;
    public CanvasGroup actText;
    public CanvasGroup dayText;

    // Sesuaikan nama scene tujuan di Inspector
    public string nextScene = "KamarJannah_Act1";

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

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        float totalLoadingTime = 7.0f; // Target waktu Loading
        float totalActTime = 5.0f;     // Target waktu Act 1
        float totalDayTime = 5.0f;     // Target waktu Day 1

        // Waktu tunggu adalah: Target Waktu - Fade In (1.0s) - Fade Out (1.0s)

        // =========================================================================
        // 1. LOADING... (Total 7.0 detik)
        // =========================================================================
        // 1.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(loadingText, true, fadeDuration));

        // 1.2. Tahan (5.0s): Waktu yang dibutuhkan agar tetap terlihat = 7.0s - 1.0s (in) - 1.0s (out)
        yield return new WaitForSeconds(totalLoadingTime - (2 * fadeDuration));

        // 1.3. Fade Out (1.0s)
        yield return StartCoroutine(FadeText(loadingText, false, fadeDuration));

        // =========================================================================
        // 2. ACT 1 (Total 5.0 detik)
        // =========================================================================
        // 2.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(actText, true, fadeDuration));

        // 2.2. Tahan (3.0s): Waktu yang dibutuhkan agar tetap terlihat = 5.0s - 1.0s (in) - 1.0s (out)
        yield return new WaitForSeconds(totalActTime - (2 * fadeDuration));

        // 2.3. Fade Out (1.0s)
        yield return StartCoroutine(FadeText(actText, false, fadeDuration));

        // =========================================================================
        // 3. DAY 1 (Total 5.0 detik)
        // =========================================================================
        // 3.1. Fade In (1.0s)
        yield return StartCoroutine(FadeText(dayText, true, fadeDuration));

        // 3.2. Tahan (4.0s): Waktu yang dibutuhkan = 5.0s - 1.0s (in)
        yield return new WaitForSeconds(totalDayTime - fadeDuration);

        // Pindah ke scene utama
        SceneManager.LoadScene(nextScene);
    }

    // Coroutine baru untuk menyederhanakan urutan Fade In dan Fade Out
    IEnumerator FadeSequence(CanvasGroup cg)
    {
        // FADE IN
        yield return StartCoroutine(FadeText(cg, true, fadeDuration));

        // JEDA (Tahan)
        yield return new WaitForSeconds(stayDuration);

        // FADE OUT
        yield return StartCoroutine(FadeText(cg, false, fadeDuration));
    }

    // Fungsi Fade Dasar (dari skrip awalmu)
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