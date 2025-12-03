using UnityEngine;

public class Act3LightingManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light directionalLight; // Main directional light (sun)
    public float darkIntensity = 0.1f; // Intensitas cahaya gelap
    public Color darkAmbientColor = new Color(0.1f, 0.1f, 0.15f); // Warna ambient gelap

    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.05f, 0.05f, 0.1f);
    public float fogDensity = 0.02f;

    [Header("Player Flashlight (Optional)")]
    public Light playerFlashlight;
    public KeyCode flashlightKey = KeyCode.L;
    public float flashlightIntensity = 3f;
    public float flashlightRange = 10f;
    private bool flashlightOn = false;

    void Start()
    {
        // Hanya aktif di Act 3
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.currentAct == 3)
        {
            SetupDarkEnvironment();
            Debug.Log("[Act3Lighting] Dark environment activated for Act 3!");
        }
        else
        {
            this.enabled = false;
            Debug.Log("[Act3Lighting] Not Act 3 - script disabled");
        }
    }

    void SetupDarkEnvironment()
    {
        // Auto-detect directional light jika tidak di-assign
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        // Set directional light menjadi sangat redup
        if (directionalLight != null)
        {
            directionalLight.intensity = darkIntensity;
            directionalLight.color = new Color(0.5f, 0.5f, 0.6f); // Slight blue tint
            Debug.Log("[Act3Lighting] Directional light dimmed");
        }

        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = darkAmbientColor;
        RenderSettings.ambientIntensity = 0.3f;

        // Setup fog
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = fogDensity;
            Debug.Log("[Act3Lighting] Fog enabled");
        }

        // Setup player flashlight jika ada
        if (playerFlashlight != null)
        {
            playerFlashlight.type = LightType.Spot;
            playerFlashlight.intensity = flashlightIntensity;
            playerFlashlight.range = flashlightRange;
            playerFlashlight.spotAngle = 45f;
            playerFlashlight.enabled = false; // Start off
            Debug.Log("[Act3Lighting] Flashlight ready (press L to toggle)");
        }
    }

    void Update()
    {
        // Toggle flashlight
        if (playerFlashlight != null && Input.GetKeyDown(flashlightKey))
        {
            flashlightOn = !flashlightOn;
            playerFlashlight.enabled = flashlightOn;
            Debug.Log("[Act3Lighting] Flashlight: " + (flashlightOn ? "ON" : "OFF"));
        }
    }

    // Function untuk restore lighting normal (jika diperlukan)
    public void RestoreNormalLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.intensity = 1f;
            directionalLight.color = Color.white;
        }

        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = false;

        Debug.Log("[Act3Lighting] Normal lighting restored");
    }

    // BONUS: Function untuk flicker lights (efek horror)
    public void FlickerLights(float duration = 0.5f)
    {
        StartCoroutine(FlickerEffect(duration));
    }

    System.Collections.IEnumerator FlickerEffect(float duration)
    {
        if (directionalLight == null) yield break;

        float originalIntensity = directionalLight.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            directionalLight.intensity = Random.Range(0f, originalIntensity * 2f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            elapsed += Time.deltaTime;
        }

        directionalLight.intensity = originalIntensity;
    }
}