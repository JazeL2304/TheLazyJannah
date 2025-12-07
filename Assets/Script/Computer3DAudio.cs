using UnityEngine;

/// <summary>
/// 3D Spatial Audio untuk Computer/Object
/// Sound akan terdengar makin keras kalau player mendekat
/// </summary>
public class Computer3DAudio : MonoBehaviour
{
    [Header("🔊 AUDIO SETTINGS")]
    [Tooltip("Sound effect yang akan diputar (keyboard typing, mouse click, dll)")]
    public AudioClip ambientSound;

    [Header("📏 DISTANCE SETTINGS")]
    [Tooltip("Jarak minimum - sound volume maksimum")]
    public float minDistance = 1f;

    [Tooltip("Jarak maksimum - sound tidak terdengar sama sekali")]
    public float maxDistance = 10f;

    [Header("🔉 VOLUME SETTINGS")]
    [Range(0f, 1f)]
    [Tooltip("Volume maksimum saat player paling dekat")]
    public float maxVolume = 0.8f;

    [Header("⚙️ PLAYBACK SETTINGS")]
    [Tooltip("Loop terus menerus (untuk ambient sound)")]
    public bool loopSound = true;

    [Tooltip("Auto play saat game start")]
    public bool playOnStart = true;

    [Header("🎨 DEBUG VISUALIZATION")]
    [Tooltip("Show gizmos di Scene view untuk visualisasi radius")]
    public bool showDebugGizmos = true;
    public Color gizmosColor = Color.cyan;

    private AudioSource audioSource;

    void Start()
    {
        // Setup AudioSource component
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ✅ CRITICAL: Setup 3D Spatial Settings!
        ConfigureAudioSource();

        // Auto play jika enabled
        if (playOnStart && ambientSound != null)
        {
            audioSource.Play();
            Debug.Log($"[3DAudio] {gameObject.name} - Sound playing!");
        }
    }

    void ConfigureAudioSource()
    {
        if (audioSource == null) return;

        // ✅ ASSIGN AUDIO CLIP
        audioSource.clip = ambientSound;

        // ✅ SET SPATIAL BLEND KE 3D (1.0 = Full 3D, 0 = 2D)
        audioSource.spatialBlend = 1f; // PENTING! Ini yang bikin 3D!

        // ✅ VOLUME ROLLOFF MODE
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Linear = smooth falloff

        // ✅ MIN & MAX DISTANCE
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        // ✅ VOLUME
        audioSource.volume = maxVolume;

        // ✅ LOOP SETTING
        audioSource.loop = loopSound;

        // ✅ PLAY ON AWAKE
        audioSource.playOnAwake = playOnStart;

        // ✅ DOPPLER EFFECT (Optional - bikin sound pitch berubah saat gerak)
        audioSource.dopplerLevel = 0.5f; // 0 = no effect, 1 = realistic

        // ✅ PRIORITY (0 = highest, 256 = lowest)
        audioSource.priority = 128; // Medium priority

        Debug.Log($"[3DAudio] {gameObject.name} configured!");
        Debug.Log($"  - Min Distance: {minDistance}m");
        Debug.Log($"  - Max Distance: {maxDistance}m");
        Debug.Log($"  - Spatial Blend: 3D (1.0)");
    }

    // Manual play/stop controls
    public void PlaySound()
    {
        if (audioSource != null && ambientSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"[3DAudio] {gameObject.name} - Playing!");
            }
        }
    }

    public void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            Debug.Log($"[3DAudio] {gameObject.name} - Stopped!");
        }
    }

    public void PauseSound()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    // Adjust volume at runtime
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }

    // Visualisasi radius di Scene view
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = gizmosColor;

        // Draw min distance sphere (volume maksimum)
        Gizmos.DrawWireSphere(transform.position, minDistance);

        // Draw max distance sphere (sound tidak terdengar)
        Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Draw line between min and max
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position + Vector3.forward * minDistance,
            transform.position + Vector3.forward * maxDistance
        );
    }

    void OnDrawGizmosSelected()
    {
        // Show detailed info when object selected
        if (!showDebugGizmos) return;

        // Draw filled sphere at min distance
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, minDistance);

        // Draw wire sphere at max distance
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }

#if UNITY_EDITOR
    // Validate settings di Inspector
    void OnValidate()
    {
        // Pastikan max distance lebih besar dari min distance
        if (maxDistance < minDistance)
        {
            maxDistance = minDistance + 1f;
        }

        // Update AudioSource jika ada perubahan
        if (audioSource != null && Application.isPlaying)
        {
            ConfigureAudioSource();
        }
    }
#endif
}