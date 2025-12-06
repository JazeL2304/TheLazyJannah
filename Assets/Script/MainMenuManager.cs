using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // (Opsional) Tambahin delay biar kayak ada efek loading
        yield return new WaitForSeconds(1.5f);
            
        // Ganti "Envirotment Game" dengan nama scene kamu
        SceneManager.LoadScene("LoadingScene");
    }
}
