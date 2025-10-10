using UnityEngine;
using System.Collections;

public class NPCStaticLookAround : MonoBehaviour
{
    [Header("Look Settings")]
    public Transform modelTransform;    // BARU: Reference ke model 3D child
    public float leftAngle = -60f;
    public float rightAngle = 0f;
    public float rotationSpeed = 2f;
    public float lookInterval = 5f;
    public bool startLookingLeft = false;

    private bool isLookingLeft = false;
    private float timer = 0f;
    private Quaternion targetRotation;
    private Quaternion baseRotation;

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

        // Rotate MODEL saja (child), BUKAN parent GameObject
        modelTransform.rotation = Quaternion.Slerp(
            modelTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

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
}
