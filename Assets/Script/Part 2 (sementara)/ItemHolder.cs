using UnityEngine;
using System.Collections.Generic;

public class ItemHolder : MonoBehaviour
{
    [Header("Hold Settings")]
    public Transform holdPoint;
    public List<GameObject> heldObjects = new List<GameObject>();
    public Camera playerCamera;

    [Header("Hold Position")]
    public Vector3 cameraLocalOffset = new Vector3(1.73f, -0.49f, 3.4f);
    public Vector3 holdRotation = new Vector3(-5f, 180f, 0f);

    [Header("Return Settings")]
    public KeyCode returnKey = KeyCode.Q;

    // WALLET DATA VARIABLES
    private Vector3 walletOriginalPosition;
    private Quaternion walletOriginalRotation;
    private Vector3 walletOriginalScale;
    private bool walletPickedUp = false;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (holdPoint == null)
        {
            GameObject holdPointGO = new GameObject("HoldPoint");
            holdPointGO.transform.SetParent(playerCamera.transform);
            holdPointGO.transform.localPosition = cameraLocalOffset;
            holdPointGO.transform.localRotation = Quaternion.Euler(holdRotation);
            holdPoint = holdPointGO.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("============================================");
            Debug.Log("[ItemHolder] Q KEY PRESSED!");
            Debug.Log("[ItemHolder] walletPickedUp: " + walletPickedUp);
            Debug.Log("[ItemHolder] heldObjects count: " + heldObjects.Count);
            foreach (GameObject obj in heldObjects)
            {
                if (obj != null)
                {
                    Debug.Log("[ItemHolder] Held object: " + obj.name);
                }
            }
            Debug.Log("============================================");
        }

        if (Input.GetKeyDown(KeyCode.Q) && walletPickedUp)
        {
            ReturnWallet();
        }
    }

    public void HoldObject(GameObject obj, Vector3 localOffset = default)
    {
        if (obj == null) return;

        Debug.Log("[ItemHolder] Holding: " + obj.name);

        // SAVE WALLET DATA **SEBELUM** SetParent!
        if (obj.name.Contains("Wallet") && !walletPickedUp)
        {
            walletOriginalPosition = obj.transform.position;
            walletOriginalRotation = obj.transform.rotation;
            walletOriginalScale = obj.transform.localScale;
            walletPickedUp = true;

            Debug.Log("========== WALLET DATA SAVED ==========");
            Debug.Log("Position: " + walletOriginalPosition);
            Debug.Log("Rotation: " + walletOriginalRotation.eulerAngles);
            Debug.Log("Scale: " + walletOriginalScale);
            Debug.Log("=======================================");

            // GAK PERLU TRIGGER MANUAL - ControlHintsManager otomatis detect!
        }

        // Disable physics
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 originalScale = obj.transform.localScale;

        // SetParent SETELAH save!
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = localOffset;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = originalScale;

        heldObjects.Add(obj);
    }
    


    public void DropObject(GameObject obj)
    {
        if (obj == null) return;

        Debug.Log("[ItemHolder] Dropping: " + obj.name);

        obj.transform.SetParent(null);

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        heldObjects.Remove(obj);
    }

    public void DropAllObjects()
    {
        foreach (GameObject obj in heldObjects)
        {
            if (obj != null)
            {
                obj.transform.SetParent(null);

                Collider col = obj.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
        }

        heldObjects.Clear();
    }

    void ReturnWallet()
    {
        Debug.Log("[ItemHolder] Mengembalikan wallet ke lemari!");

        GameObject wallet = GetHeldObject("Wallet");

        if (wallet != null)
        {
            // Destroy credit card jika ada
            GameObject card = GetHeldObject("CreditCard");
            if (card != null)
            {
                heldObjects.Remove(card);
                Destroy(card);
                Debug.Log("[ItemHolder] Credit card destroyed");
            }

            // Remove wallet dari list
            heldObjects.Remove(wallet);

            // HARDCODE POSISI WALLET YANG BARU!
            wallet.transform.SetParent(null);
            wallet.transform.position = new Vector3(65.278f, 11.389f, 31.233f);
            wallet.transform.rotation = Quaternion.Euler(-70f, 90f, 0f);
            wallet.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            Debug.Log("[ItemHolder] Wallet returned to: (65.278, 11.389, 31.233)");

            // Enable collider & physics
            Collider col = wallet.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            Rigidbody rb = wallet.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            // RESET INTERACTABLE OBJECT
            InteractableObject interactable = wallet.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.ResetInteraction();
                Debug.Log("[ItemHolder] InteractableObject reset!");
            }

            // RESET CREDIT CARD MANAGER
            CreditCardManager cardManager = GetComponent<CreditCardManager>();
            if (cardManager != null)
            {
                cardManager.ResetCardState();
            }

            walletPickedUp = false;

            Debug.Log("[ItemHolder] Wallet dikembalikan - DONE!");
        }
    }

    public GameObject GetHeldObject(string nameContains)
    {
        foreach (GameObject obj in heldObjects)
        {
            if (obj != null && obj.name.Contains(nameContains))
            {
                return obj;
            }
        }
        return null;
    }

    public GameObject currentHeldObject
    {
        get { return heldObjects.Count > 0 ? heldObjects[0] : null; }
    }

    public void OverrideWalletScale(Vector3 newScale)
    {
        walletOriginalScale = newScale;
        Debug.Log("[ItemHolder] Wallet scale OVERRIDDEN to: " + newScale);
    }

    public void SetWalletOriginalData(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        walletOriginalPosition = pos;
        walletOriginalRotation = rot;
        walletOriginalScale = scale;
        walletPickedUp = true;

        Debug.Log("[ItemHolder] ===== WALLET DATA SET FROM INTERACTABLE =====");
        Debug.Log("Position: " + pos);
        Debug.Log("Rotation: " + rot.eulerAngles);
        Debug.Log("Scale: " + scale);
        Debug.Log("========================================================");
    }

    public void HideHeldObjects()
    {
        Debug.Log("[ItemHolder] Hiding wallet only (credit card stays visible)...");

        foreach (GameObject obj in heldObjects)
        {
            if (obj != null && obj.name.Contains("Wallet"))
            {
                obj.SetActive(false);
                Debug.Log("[ItemHolder] Hidden: " + obj.name);
            }
        }
    }

    public void ShowHeldObjects()
    {
        Debug.Log("[ItemHolder] Showing wallet...");

        foreach (GameObject obj in heldObjects)
        {
            if (obj != null && obj.name.Contains("Wallet"))
            {
                obj.SetActive(true);
                Debug.Log("[ItemHolder] Shown: " + obj.name);
            }
        }
    }

    // FIXED - Gunakan foreach untuk List, bukan ContainsKey!
    public bool IsHoldingWallet()
    {
        foreach (GameObject obj in heldObjects)
        {
            if (obj != null && obj.name.Contains("Wallet"))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHoldingCreditCard()
    {
        foreach (GameObject obj in heldObjects)
        {
            if (obj != null && (obj.name.Contains("CreditCard") || obj.name.Contains("Kartu")))
            {
                return true;
            }
        }
        return false;
    }
}
    