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

        // ✅ RESET PROGRESS KE ACT 1 (OPSIONAL - BISA DIATUR DI INSPECTOR)
        if (resetProgressOnMainMenu && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
            Debug.Log("[MainMenu] ✅ Progress reset to ACT 1 DAY 1!");
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