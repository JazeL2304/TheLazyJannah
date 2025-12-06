using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // ✅ PASTIKAN GAMEPROGRESSMANAGER ADA (tapi JANGAN RESET!)
        if (GameProgressManager.Instance == null)
        {
            Debug.Log("[MainMenu] GameProgressManager not found - Creating new one...");

            GameObject gpm = new GameObject("GameProgressManager");
            gpm.AddComponent<GameProgressManager>();

            Debug.Log("[MainMenu] GameProgressManager created!");
        }

        // ✅ UNLOCK CURSOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ✅ LOG CURRENT PROGRESS (JANGAN RESET DI SINI!)
        if (GameProgressManager.Instance != null)
        {
            Debug.Log($"[MainMenu] Current Progress: ACT {GameProgressManager.Instance.currentAct} DAY {GameProgressManager.Instance.currentDay}");
        }
    }

    public void StartGame()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // ✅ RESET CUMA SAAT START GAME BARU!
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
            Debug.Log("[MainMenu] Starting NEW GAME - Progress reset to ACT 1 DAY 1");
        }

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("LoadingScene");
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