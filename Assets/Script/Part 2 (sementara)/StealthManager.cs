using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StealthManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI restartText;  // TAMBAHAN BARU

    [Header("Checkpoint")]
    public Transform currentCheckpoint;

    private bool isDetected = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void OnPlayerDetected(string detectorName)
    {
        if (isDetected) return;

        isDetected = true;
        Debug.Log("Player tertangkap oleh: " + detectorName);

        StartCoroutine(ShowGameOver(detectorName));
    }

    IEnumerator ShowGameOver(string detectorName)
    {
        // Freeze player
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // HANYA TERTANGKAP!
            if (gameOverText != null)
            {
                gameOverText.text = "TERTANGKAP!";
            }

            // TAMPILKAN RESTART TEXT
            if (restartText != null)
            {
                restartText.text = "Tekan R untuk restart";
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;
    }

    void Update()
    {
        if (isDetected && Input.GetKeyDown(KeyCode.R))
        {
            RestartFromCheckpoint();
        }
    }

    void RestartFromCheckpoint()
    {
        Debug.Log("Restart dari checkpoint");

        if (currentCheckpoint != null && player != null)
        {
            // Teleport player ke checkpoint
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = currentCheckpoint.position;
                player.transform.rotation = currentCheckpoint.rotation;
                cc.enabled = true;
            }

            // Reset player controller
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
        else
        {
            // Reload scene jika tidak ada checkpoint
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isDetected = false;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
        Debug.Log("Checkpoint diset: " + checkpoint.name);
    }
}
