using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    public Animator doorAnimator;
    public GameObject interactionUI;

    void Update()
    {
        // Hanya cek input jika pemain berada di dalam jangkauan
        if (isPlayerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithDoor(); // Panggil method yang sudah ada
            }
        }
    }

    // METHOD YANG DIPERLUKAN OLEH PLAYERCONTROLLER
    public void InteractWithDoor()
    {
        // Periksa status animasi pintu dari parameter Animator
        bool isOpen = doorAnimator.GetBool("IsOpen");
        // Ubah status animasi (dari terbuka ke tertutup, dan sebaliknya)
        doorAnimator.SetBool("IsOpen", !isOpen);
        // Pesan konfirmasi
        Debug.Log("Interaksi pintu berhasil dengan tombol E!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Deteksi jika pemain masuk ke area trigger
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Pemain terdeteksi di dekat pintu!");
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
            isPlayerInRange = false;
            Debug.Log("Pemain keluar dari area pintu.");
            // Sembunyikan UI interaksi
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}