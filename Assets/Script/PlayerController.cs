using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float moveSpeed = 5f;
    public float lookSensitivity = 1f;
    public float gravity = -9.81f;

    [Header("Animation")]
    public Animator animator;

    [Header("Dialogue Settings")]
    public Dialogue dialogueManager;  // TAMBAHAN BARU

    private Camera playerCamera;
    private Vector3 moveDirection;
    private Vector3 playerVelocity;
    private float xRotation = 0f;

    private DoorInteraction currentDoor;
    private ChairInteraction currentChair;
    private PlayerChairController chairController;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;

        chairController = GetComponent<PlayerChairController>();

        // AUTO-DETECT ANIMATOR
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Debug.Log("Animator otomatis terdeteksi!");
            }
        }

        // AUTO-DETECT DIALOGUE MANAGER
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Dialogue>();
        }
    }

    void Update()
    {
        // CEK APAKAH DIALOG SEDANG BERLANGSUNG - BLOKIR SEMUA INPUT!
        if (dialogueManager != null && dialogueManager.IsDialogueActive())
        {
            // Stop animasi saat dialog
            if (animator != null)
            {
                animator.speed = 0f;
            }
            return; // BLOKIR SEMUA KONTROL
        }

        // **Cek jika sedang duduk**
        bool isSitting = (chairController != null && chairController.IsSitting());

        if (isSitting)
        {
            // STOP ANIMASI SAAT DUDUK
            if (animator != null)
            {
                animator.speed = 0f;
            }

            // Tetap bisa berdiri dengan menekan F
            if (Input.GetKeyDown(KeyCode.F))
            {
                chairController.StandUp();
                Debug.Log("Player berdiri dari kursi!");
            }
            return; // Blokir semua kontrol lain
        }

        // ===== MOVEMENT (WASD) =====
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        moveDirection = transform.right * x + transform.forward * z;

        // CEK APAKAH SEDANG BERGERAK
        bool isMoving = moveDirection.magnitude > 0.1f;

        // KONTROL ANIMASI WALK (PLAY/STOP)
        if (animator != null)
        {
            if (isMoving)
            {
                animator.speed = 1f; // PLAY animation normal
            }
            else
            {
                animator.speed = 0f; // STOP/FREEZE animation
            }
        }

        // Gravitasi
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime + playerVelocity * Time.deltaTime);

        // ===== ROTASI KAMERA =====
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // ===== INTERAKSI PINTU (Tombol E) =====
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentDoor != null)
            {
                currentDoor.InteractWithDoor();
                Debug.Log("Mencoba membuka/menutup pintu...");
            }
            else
            {
                Debug.Log("Tidak ada pintu di sekitar!");
            }
        }

        // ===== INTERAKSI KURSI (Tombol F) =====
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentChair != null && chairController != null)
            {
                currentChair.InteractWithChair(chairController);
                Debug.Log("Mencoba duduk di kursi...");
            }
            else
            {
                Debug.Log("Tidak ada kursi di sekitar!");
            }
        }
    }

    // Fungsi untuk set door yang sedang di-trigger
    public void SetCurrentDoor(DoorInteraction door)
    {
        currentDoor = door;
        Debug.Log("Current Door: " + (door != null ? door.gameObject.name : "NULL"));
    }

    // Fungsi untuk set chair yang sedang di-trigger
    public void SetCurrentChair(ChairInteraction chair)
    {
        currentChair = chair;
        Debug.Log("Current Chair: " + (chair != null ? chair.gameObject.name : "NULL"));
    }
}
