using UnityEngine;
using System.Collections;

public class NPCStaticLookAround : MonoBehaviour
{
    [Header("Look Settings")]
    public Transform modelTransform;    // Reference ke model 3D child
    public float leftAngle = -60f;
    public float rightAngle = 0f;
    public float rotationSpeed = 2f;
    public float lookInterval = 5f;
    public bool startLookingLeft = false;

    [Header("Animation Settings")]
    public Animator animator;  // TAMBAHAN untuk animasi
    public float angleThreshold = 5f;  // TAMBAHAN - Threshold untuk stop animasi (default 5 derajat)

    private bool isLookingLeft = false;
    private float timer = 0f;
    private Quaternion targetRotation;
    private Quaternion baseRotation;
    private bool isTurning = false;  // Flag untuk track apakah sedang putar

    void Start()
    {
        // Auto-detect model jika tidak di-assign
        if (modelTransform == null)
        {
            // Cari child pertama yang punya mesh renderer
            foreach (Transform child in transform)
            {
                if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null ||
                    child.GetComponentInChildren<MeshRenderer>() != null)
                {
                    modelTransform = child;
                    break;
                }
            }
        }

        if (modelTransform == null)
        {
            Debug.LogError("NPCStaticLookAround: Model Transform tidak ditemukan! Assign manual di Inspector.");
            return;
        }

        // AUTO-DETECT ANIMATOR
        if (animator == null)
        {
            animator = modelTransform.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelTransform.GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                Debug.Log("Animator otomatis terdeteksi untuk " + gameObject.name);
            }
        }

        // Simpan rotasi awal MODEL (bukan parent)
        baseRotation = modelTransform.rotation;

        // Set arah awal
        if (startLookingLeft)
        {
            targetRotation = baseRotation * Quaternion.Euler(0, leftAngle, 0);
            isLookingLeft = true;
        }
        else
        {
            targetRotation = baseRotation * Quaternion.Euler(0, rightAngle, 0);
            isLookingLeft = false;
        }
    }

    void Update()
    {
        if (modelTransform == null) return;

        // CEK ANGLE KE TARGET
        float angleToTarget = Quaternion.Angle(modelTransform.rotation, targetRotation);

        // JIKA SUDAH DEKAT THRESHOLD, LANGSUNG SNAP KE TARGET (INSTANT STOP)
        if (angleToTarget < angleThreshold)
        {
            modelTransform.rotation = targetRotation; // SNAP langsung ke target!

            // Stop animation jika masih turning
            if (isTurning)
            {
                isTurning = false;
                StopTurnAnimation();
            }
        }
        else // Masih jauh dari target, rotate smooth
        {
            // Rotate MODEL saja (child), BUKAN parent GameObject
            modelTransform.rotation = Quaternion.Slerp(
                modelTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Play animation jika belum turning
            if (!isTurning)
            {
                isTurning = true;
                PlayTurnAnimation();
            }
        }

        // Timer untuk ganti arah
        timer += Time.deltaTime;

        if (timer >= lookInterval)
        {
            timer = 0f;

            // Toggle antara kiri dan kanan
            if (isLookingLeft)
            {
                // Hadap kanan/depan
                targetRotation = baseRotation * Quaternion.Euler(0, rightAngle, 0);
                isLookingLeft = false;
            }
            else
            {
                // Hadap kiri
                targetRotation = baseRotation * Quaternion.Euler(0, leftAngle, 0);
                isLookingLeft = true;
            }
        }
    }

    void PlayTurnAnimation()
    {
        Debug.Log(gameObject.name + " - PLAY TURN! Speed = 1");

        if (animator != null)
        {
            animator.SetFloat("Speed", 1f);
            Debug.Log("Speed parameter set to 1");
        }
        else
        {
            Debug.LogError(gameObject.name + " - ANIMATOR IS NULL!");
        }
    }

    void StopTurnAnimation()
    {
        Debug.Log(gameObject.name + " - STOP TURN! Speed = 0");

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}
