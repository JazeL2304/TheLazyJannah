using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Player Reference")]
    public GameObject player; // Drag player GameObject

    [Header("Spawn Points")]
    public Transform act1SpawnPoint; // Posisi spawn Act 1
    public Transform act2SpawnPoint; // Posisi spawn Act 2
    public Transform act3SpawnPoint; // Posisi spawn Act 3 (optional)

    void Start()
    {
        TeleportPlayerToSpawn();
    }

    void TeleportPlayerToSpawn()
    {
        if (player == null)
        {
            Debug.LogError("[PlayerSpawn] Player not assigned!");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[PlayerSpawn] GameProgressManager not found! Using default spawn.");
            return;
        }

        int currentAct = GameProgressManager.Instance.currentAct;

        // Tentukan spawn point berdasarkan Act
        Transform spawnPoint = null;

        if (currentAct == 1 && act1SpawnPoint != null)
        {
            spawnPoint = act1SpawnPoint;
        }
        else if (currentAct == 2 && act2SpawnPoint != null)
        {
            spawnPoint = act2SpawnPoint;
        }
        else if (currentAct >= 3 && act3SpawnPoint != null)
        {
            spawnPoint = act3SpawnPoint;
        }

        // Teleport player
        if (spawnPoint != null)
        {
            // Get CharacterController (kalau ada)
            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false; // Disable biar bisa teleport
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                controller.enabled = true; // Enable lagi
            }
            else
            {
                // Kalau ga pake CharacterController
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
            }

            Debug.Log($"[PlayerSpawn] ✅ Player teleported to ACT {currentAct} spawn point!");
        }
        else
        {
            Debug.LogWarning($"[PlayerSpawn] ⚠️ Spawn point for ACT {currentAct} not assigned! Player stays at default position.");
        }
    }
}
