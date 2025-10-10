using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlinkController : MonoBehaviour
{
    private Image blackScreen;
    public float blinkDuration = 0.5f; // Durasi kedipan

    void Awake()
    {
        // Panggil Awake untuk memastikan inisialisasi dilakukan sebelum Start
        blackScreen = GetComponent<Image>();

        // **BARIS PENTING:** Atur warna menjadi hitam pekat segera
        blackScreen.color = new Color(0, 0, 0, 1f);

        gameObject.SetActive(true); // Pastikan objek aktif saat mulai
    }

    void Start()
    {
        // Mulai transisi mata terbuka saat scene dimulai
        StartCoroutine(OpenEyes());
    }

    IEnumerator OpenEyes()
    {
        float timer = 0f;

        // Fade out (dari hitam pekat ke transparan)
        while (timer < blinkDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / blinkDuration);
            blackScreen.color = new Color(0, 0, 0, alpha); // Turunkan Alpha
            timer += Time.deltaTime;
            yield return null;
        }

        // Pastikan transparan penuh dan sembunyikan objek
        blackScreen.color = new Color(0, 0, 0, 0);
        gameObject.SetActive(false);
    }
}