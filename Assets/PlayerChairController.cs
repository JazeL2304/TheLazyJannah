using UnityEngine;

public class PlayerChairController : MonoBehaviour
{
    [Header("Sitting Settings")]
    public float sitSpeed = 5f;

    private bool isSitting = false;
    private Transform currentChair;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // PERBAIKAN: Hapus Update() - biar PlayerController yang handle input

    public void SitOnChair(Transform chairSitPosition)
    {
        if (!isSitting)
        {
            isSitting = true;
            currentChair = chairSitPosition;

            // Simpan posisi dan rotasi asli
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            // Nonaktifkan kontrol player
            DisablePlayerControls();

            // Pindahkan player ke posisi duduk
            StartCoroutine(MoveToSitPosition(chairSitPosition));

            Debug.Log("Duduk di kursi berhasil!");
        }
    }

    public void StandUp()
    {
        if (isSitting)
        {
            isSitting = false;

            // Lepas parent dari kursi
            transform.parent = null;

            // PERBAIKAN: Kembalikan posisi sedikit di belakang kursi
            if (currentChair != null)
            {
                Vector3 standPosition = currentChair.position - currentChair.forward * 0.5f;
                transform.position = standPosition;
            }

            currentChair = null;

            // Aktifkan kembali kontrol player
            EnablePlayerControls();

            Debug.Log("Berdiri dari kursi berhasil!");
        }
    }

    public bool IsSitting()
    {
        return isSitting;
    }

    private System.Collections.IEnumerator MoveToSitPosition(Transform sitPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Nonaktifkan CharacterController sementara untuk teleport
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * sitSpeed;

            // Lerp posisi dan rotasi
            transform.position = Vector3.Lerp(startPos, sitPosition.position, elapsedTime);
            transform.rotation = Quaternion.Lerp(startRot, sitPosition.rotation, elapsedTime);

            yield return null;
        }

        // Pastikan posisi final tepat
        transform.position = sitPosition.position;
        transform.rotation = sitPosition.rotation;

        // Set parent ke kursi agar ikut bergerak jika kursi bergerak
        transform.parent = sitPosition;
    }

    private void DisablePlayerControls()
    {
        // Nonaktifkan CharacterController
        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    private void EnablePlayerControls()
    {
        // Aktifkan CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}