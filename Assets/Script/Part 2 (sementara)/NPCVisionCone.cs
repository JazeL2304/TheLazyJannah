using UnityEngine;

public class NPCVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Detection Settings")]
    public float detectionDelay = 1f;

    [Header("Visual Settings")]
    public bool showVisionCone = true;
    public Color visionConeColor = new Color(1, 0, 0, 0.3f);
    public int visionConeResolution = 20;

    private Transform player;
    private StealthManager stealthManager;
    private bool playerInSight = false;
    private float detectionTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        stealthManager = FindObjectOfType<StealthManager>();

        if (player == null)
        {
            Debug.LogError("Player tidak ditemukan! Pastikan Player punya Tag 'Player'");
        }
    }

    void Update()
    {
        if (player == null) return;

        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        playerInSight = false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Cek jarak
        if (distanceToPlayer <= viewRadius)
        {
            // Cek sudut
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= viewAngle / 2f)
            {
                // Raycast untuk cek obstacle
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    playerInSight = true;
                    detectionTimer += Time.deltaTime;

                    if (detectionTimer >= detectionDelay)
                    {
                        PlayerDetected();
                    }
                }
                else
                {
                    detectionTimer = 0f;
                }
            }
            else
            {
                detectionTimer = 0f;
            }
        }
        else
        {
            detectionTimer = 0f;
        }
    }

    void PlayerDetected()
    {
        Debug.Log(gameObject.name + " mendeteksi player!");

        if (stealthManager != null)
        {
            stealthManager.OnPlayerDetected(gameObject.name);
        }

        // Reset timer
        detectionTimer = 0f;
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void OnDrawGizmos()
    {
        if (!showVisionCone) return;

        Gizmos.color = visionConeColor;

        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2, false);

        // Draw view radius
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        // Draw vision cone arc
        Vector3 previousPoint = transform.position + viewAngleA * viewRadius;

        for (int i = 1; i <= visionConeResolution; i++)
        {
            float angle = Mathf.Lerp(-viewAngle / 2, viewAngle / 2, i / (float)visionConeResolution);
            Vector3 direction = DirectionFromAngle(angle, false);
            Vector3 point = transform.position + direction * viewRadius;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Highlight if player in sight
        if (Application.isPlaying && playerInSight)
        {
            Gizmos.color = Color.red;
            if (player != null)
            {
                Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
            }
        }
    }

    public bool IsPlayerInSight()
    {
        return playerInSight;
    }

    public float GetDetectionProgress()
    {
        return Mathf.Clamp01(detectionTimer / detectionDelay);
    }
}