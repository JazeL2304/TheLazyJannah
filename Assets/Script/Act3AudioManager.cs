using UnityEngine;

public class Act3AudioManager : MonoBehaviour
{
    [Header("Horror Audio Settings")]
    public AudioClip horrorBackgroundMusic;
    public AudioClip horrorAmbience; // Suara ambient (angin, derit, dll)

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float ambienceVolume = 0.3f;

    [Header("Fade Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1f;

    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private AudioSource oldBackgroundMusic; // Reference ke background music lama

    void Start()
    {
        // Hanya aktif di Act 3
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.currentAct == 3)
        {
            SetupAct3Audio();
        }
        else
        {
            Debug.Log("[Act3Audio] Not Act 3 - script disabled");
            this.enabled = false;
        }
    }

    void SetupAct3Audio()
    {
        // STEP 1: CARI & MATIKAN BACKGROUND MUSIC LAMA
        BackgroundMusic oldBGM = FindObjectOfType<BackgroundMusic>();
        if (oldBGM != null)
        {
            AudioSource oldSource = oldBGM.GetComponent<AudioSource>();
            if (oldSource != null)
            {
                oldBackgroundMusic = oldSource;
                StartCoroutine(FadeOutAudio(oldSource, fadeOutDuration));
                Debug.Log("[Act3Audio] Old background music fading out...");
            }
        }

        // STEP 2: SETUP AUDIO SOURCE BARU UNTUK HORROR MUSIC
        if (horrorBackgroundMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = horrorBackgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0f; // Start dari 0 untuk fade in
            musicSource.Play();

            StartCoroutine(FadeInAudio(musicSource, musicVolume, fadeInDuration));
            Debug.Log("[Act3Audio] Horror background music playing!");
        }

        // STEP 3: SETUP AUDIO SOURCE UNTUK HORROR AMBIENCE
        if (horrorAmbience != null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.clip = horrorAmbience;
            ambienceSource.loop = true;
            ambienceSource.volume = 0f;
            ambienceSource.Play();

            StartCoroutine(FadeInAudio(ambienceSource, ambienceVolume, fadeInDuration + 0.5f));
            Debug.Log("[Act3Audio] Horror ambience playing!");
        }
    }

    System.Collections.IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();

        // Disable BackgroundMusic script agar tidak play lagi
        BackgroundMusic bgm = source.GetComponent<BackgroundMusic>();
        if (bgm != null)
        {
            bgm.enabled = false;
        }
    }

    System.Collections.IEnumerator FadeInAudio(AudioSource source, float targetVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    // Function untuk restore music normal (jika diperlukan)
    public void RestoreNormalMusic()
    {
        // Fade out horror sounds
        if (musicSource != null)
        {
            StartCoroutine(FadeOutAndDestroy(musicSource, fadeOutDuration));
        }

        if (ambienceSource != null)
        {
            StartCoroutine(FadeOutAndDestroy(ambienceSource, fadeOutDuration));
        }

        // Restore old background music
        if (oldBackgroundMusic != null)
        {
            BackgroundMusic bgm = oldBackgroundMusic.GetComponent<BackgroundMusic>();
            if (bgm != null)
            {
                bgm.enabled = true;
            }

            oldBackgroundMusic.Play();
            StartCoroutine(FadeInAudio(oldBackgroundMusic, 0.5f, fadeInDuration));
            Debug.Log("[Act3Audio] Normal music restored");
        }
    }

    System.Collections.IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
    {
        yield return StartCoroutine(FadeOutAudio(source, duration));
        Destroy(source);
    }

    void OnDestroy()
    {
        // Cleanup saat pindah scene
        if (musicSource != null) Destroy(musicSource);
        if (ambienceSource != null) Destroy(ambienceSource);
    }
}