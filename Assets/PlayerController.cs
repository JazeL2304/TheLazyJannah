using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public float moveSpeed = 5f;
    public float lookSensitivity = 1f;
    public float gravity = -9.81f;

    private Camera playerCamera;
    private Vector3 moveDirection;
    private Vector3 playerVelocity;
    private float xRotation = 0f;
    private DoorInteraction currentDoor; // Variabel untuk menyimpan referensi pintu

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Pergerakan (WASD)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        moveDirection = transform.right * x + transform.forward * z;

        // **LOGIKA GRAVITASI**
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;

        // Gabungkan gerakan horizontal dan vertikal
        controller.Move(moveDirection * moveSpeed * Time.deltaTime + playerVelocity * Time.deltaTime);

        // **AKHIR LOGIKA GRAVITASI**

        // Rotasi Kamera (Look around)
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // **LOGIKA INTERAKSI PINTU**
        // Cek apakah tombol 'E' ditekan dan ada pintu di dekatnya
        if (Input.GetKeyDown(KeyCode.E) && currentDoor != null)
        {
            currentDoor.InteractWithDoor();
        }
    }

    // Fungsi ini dipanggil saat Character Controller menabrak collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Cek apakah objek yang ditabrak memiliki tag "Doorway"
        if (hit.gameObject.CompareTag("Doorway"))
        {
            // Simpan referensi skrip pintu
            currentDoor = hit.gameObject.GetComponent<DoorInteraction>();
        }
    }
}