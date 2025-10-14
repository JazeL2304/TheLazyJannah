using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Animator doorAnimator;
    public GameObject interactionUI;

    // TAMBAHAN BARU: Property untuk cek status pintu (diakses dari luar)
    public bool isOpen
    {
        get
        {
            if (doorAnimator != null)
            {
                return doorAnimator.GetBool("IsOpen");
            }
            return false;
        }
    }

    // Method yang dipanggil dari PlayerController
    public void InteractWithDoor()
    {
        // Periksa status animasi pintu dari parameter Animator
        bool currentState = doorAnimator.GetBool("IsOpen");

        // Ubah status animasi (dari terbuka ke tertutup, dan sebaliknya)
        doorAnimator.SetBool("IsOpen", !currentState);

        // Pesan konfirmasi
        Debug.Log("Pintu " + (currentState ? "ditutup" : "dibuka") + "!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Deteksi jika pemain masuk ke area trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pemain terdeteksi di dekat pintu!");

            // Beritahu PlayerController bahwa ini pintu aktif
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCurrentDoor(this);
            }

            // Tampilkan UI interaksi jika ada
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Deteksi jika pemain keluar dari area trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pemain keluar dari area pintu.");

            // Beritahu PlayerController bahwa tidak ada pintu aktif
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCurrentDoor(null);
            }

            // Sembunyikan UI interaksi
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}
