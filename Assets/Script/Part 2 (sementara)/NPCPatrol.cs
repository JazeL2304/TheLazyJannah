using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float rotationSpeed = 3f;
    public float waitTime = 2f;

    [Header("Look Around Settings")]
    public bool enableLookAround = true;
    public float lookLeftAngle = -90f;
    public float lookRightAngle = 90f;
    public float lookWaitTime = 3f;

    [Header("Physics Settings")]
    public float gravity = -9.81f;
    public LayerMask groundLayer;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isLookingAround = false;
    private float waitTimer = 0f;
    private Quaternion originalRotation;

    // TAMBAHAN UNTUK GRAVITY
    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        // TAMBAHKAN CHARACTER CONTROLLER
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("NPCPatrol membutuhkan CharacterController! Menambahkan otomatis...");
            controller = gameObject.AddComponent<CharacterController>();

            // Setup default CharacterController untuk NPC
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 0.9f, 0);
        }

        if (waypoints.Length > 0)
        {
            originalRotation = transform.rotation;
        }
    }

    void Update()
    {
        // APPLY GRAVITY
        if (controller.isGrounded)
        {
            velocity.y = -2f; // Sedikit force ke bawah
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Move dengan gravity
        controller.Move(velocity * Time.deltaTime);

        // Patrol logic
        if (waypoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else if (enableLookAround && !isLookingAround)
            {
                StartCoroutine(LookAround());
            }
        }
        else
        {
            MoveToWaypoint();
        }
    }

    void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Rotate menuju waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0; // PENTING! Jangan rotate ke atas/bawah

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // Bergerak ke waypoint (horizontal only)
        Vector3 moveDirection = (targetWaypoint.position - transform.position).normalized;
        moveDirection.y = 0; // Jangan gerak vertikal

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Cek jika sudah sampai (ignore Y axis)
        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(targetWaypoint.position.x, 0, targetWaypoint.position.z);

        if (Vector3.Distance(flatPos, flatTarget) < 0.5f)
        {
            isWaiting = true;
        }
    }

    System.Collections.IEnumerator LookAround()
    {
        isLookingAround = true;
        originalRotation = transform.rotation;

        // Lihat kiri
        Quaternion leftRotation = originalRotation * Quaternion.Euler(0, lookLeftAngle, 0);
        float timer = 0f;

        while (timer < lookWaitTime / 2)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, leftRotation, rotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Lihat kanan
        Quaternion rightRotation = originalRotation * Quaternion.Euler(0, lookRightAngle, 0);
        timer = 0f;

        while (timer < lookWaitTime / 2)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rightRotation, rotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Kembali ke depan
        while (Quaternion.Angle(transform.rotation, originalRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        isLookingAround = false;
    }
}
