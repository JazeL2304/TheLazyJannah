using UnityEngine;

public class PlayerInteractionRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Camera playerCamera;
    public float rayDistance = 5f;
    public LayerMask interactableLayer;  // Layer untuk object yang bisa di-interact

    [Header("Input")]
    public KeyCode interactionKey = KeyCode.E;

    private InteractableObject currentInteractable;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // Raycast dari center screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                // Show prompt jika object baru
                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.HidePrompt();
                    }

                    currentInteractable = interactable;
                    currentInteractable.ShowPrompt();
                }

                // Check for interaction input
                if (Input.GetKeyDown(interactionKey))
                {
                    currentInteractable.Interact();
                    currentInteractable = null;  // Clear after interact
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }
    }
}
