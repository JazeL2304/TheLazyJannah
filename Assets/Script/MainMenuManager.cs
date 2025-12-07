using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("🔄 RESET SETTINGS")]
    public bool resetProgressOnMainMenu = true; // Reset progress saat ke main menu

    void Start()
    {
        // ✅ PASTIKAN CURSOR UNLOCKED DI MAIN MENU!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[MainMenu] Main Menu loaded - Cursor unlocked!");

        // ❌ JANGAN RESET OTOMATIS! Cuma reset kalau user klik Start Game!
        // User mungkin balik ke main menu dari in-game untuk setting, dll
        if (resetProgressOnMainMenu)
        {
            Debug.LogWarning("[MainMenu] ⚠️ resetProgressOnMainMenu = TRUE (not recommended!)");
            // KOMENTAR BARIS INI AGAR TIDAK AUTO-RESET!
            // GameProgressManager.Instance.ResetProgress();
        }
    }

    void Update()
    {
        // ✅ SAFETY CHECK - Pastikan cursor selalu visible di main menu
        if (!Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void StartGame()
    {
        Debug.Log("[MainMenu] Start Game clicked!");

        // ✅ RESET PROGRESS SEBELUM MULAI GAME BARU
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
            Debug.Log("[MainMenu] Progress reset - Starting fresh game!");
        }

        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // Delay untuk efek loading
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[MainMenu] Loading game scene...");

        // Load ke loading scene
        SceneManager.LoadScene("LoadingScene");
    }

    // ✅ FUNCTION BARU - Reset manual dari script lain
    public void ResetToMainMenu()
    {
        Debug.Log("[MainMenu] Manual reset triggered!");

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load main menu
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}