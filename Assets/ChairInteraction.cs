using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPosition;
    public Vector3 sitOffset = new Vector3(0, 0.5f, 0);
    public GameObject interactionUI;

    void Start()
    {
        if (sitPosition == null)
        {
            GameObject sitPos = new GameObject("SitPosition");
            sitPos.transform.parent = transform;
            sitPos.transform.localPosition = sitOffset;
            sitPos.transform.localRotation = Quaternion.identity;
            sitPosition = sitPos.transform;
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    // PERBAIKAN: Method yang dipanggil dari PlayerController
    public void InteractWithChair(PlayerChairController playerController)
    {
        if (playerController != null && !playerController.IsSitting())
        {
            playerController.SitOnChair(sitPosition);
            Debug.Log("Player duduk di kursi!");

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pemain terdeteksi di dekat kursi!");

            PlayerChairController playerController = other.GetComponent<PlayerChairController>();

            // PERBAIKAN: Beritahu PlayerController bahwa ini kursi aktif
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetCurrentChair(this);
            }

            // Tampilkan UI HANYA jika player tidak sedang duduk
            if (playerController != null && !playerController.IsSitting())
            {
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pemain keluar dari area kursi.");

            // PERBAIKAN: Beritahu PlayerController bahwa tidak ada kursi aktif
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetCurrentChair(null);
            }

            // Sembunyikan UI saat keluar
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    public Transform GetSitPosition()
    {
        return sitPosition;
    }
}