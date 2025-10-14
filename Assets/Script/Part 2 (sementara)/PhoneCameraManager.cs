using UnityEngine;

public class PhoneCameraManager : MonoBehaviour
{
    [Header("Phone Settings")]
    public GameObject phonePrefab;
    public KeyCode openPhoneKey = KeyCode.C;
    public KeyCode takePhotoKey = KeyCode.Space;

    [Header("Position Offset")]
    public Vector3 phoneOffset = new Vector3(0.2f, -0.1f, 0f);

    [Header("References")]
    public ItemHolder itemHolder;
    public QuestManager questManager;
    public Camera playerCamera;
    public DialoguePhotoManager dialoguePhotoManager;

    [Header("Quest Settings")]
    public int questIndex = 0;
    public int objectiveIndex = 2;

    [Header("UI")]
    public GameObject cameraUI;
    public GameObject photoFlash;

    [Header("Sound Effects")] // ← BARU!
    public AudioClip cameraShutterSound;
    private AudioSource audioSource;

    private bool phoneActive = false;
    private GameObject currentPhone;

    void Start()
    {
        if (itemHolder == null)
        {
            itemHolder = FindObjectOfType<ItemHolder>();
        }

        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (dialoguePhotoManager == null)
        {
            dialoguePhotoManager = FindObjectOfType<DialoguePhotoManager>();
        }

        if (cameraUI != null)
        {
            cameraUI.SetActive(false);
        }

        if (photoFlash != null)
        {
            photoFlash.SetActive(false);
        }

        // SETUP AUDIOSOURCE (BARU!)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(openPhoneKey) && !phoneActive)
        {
            OpenPhone();
        }
        else if (Input.GetKeyDown(openPhoneKey) && phoneActive)
        {
            ClosePhone();
        }

        if (Input.GetKeyDown(takePhotoKey) && phoneActive)
        {
            TakePhoto();
        }
    }

    void OpenPhone()
    {
        phoneActive = true;

        Debug.Log("[Phone] HP camera dibuka!");

        if (itemHolder != null)
        {
            itemHolder.HideHeldObjects();
        }

        if (phonePrefab != null && itemHolder != null)
        {
            Vector3 spawnPos = itemHolder.holdPoint.position;
            currentPhone = Instantiate(phonePrefab, spawnPos, Quaternion.identity);
            currentPhone.name = "Phone";

            currentPhone.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Vector3 phoneLocalOffset = new Vector3(-0.016f, -0.003f, -0.003f);
            itemHolder.HoldObject(currentPhone, phoneLocalOffset);

            currentPhone.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Debug.Log("[Phone] Phone positioned with scale (0.2, 0.2, 0.2)");
        }

        if (cameraUI != null)
        {
            cameraUI.SetActive(true);
        }
    }

    void ClosePhone()
    {
        phoneActive = false;

        Debug.Log("[Phone] HP camera ditutup!");

        if (currentPhone != null)
        {
            itemHolder.DropObject(currentPhone);
            Destroy(currentPhone);
        }

        if (cameraUI != null)
        {
            cameraUI.SetActive(false);
        }

        if (itemHolder != null)
        {
            itemHolder.ShowHeldObjects();
        }
    }

    void TakePhoto()
    {
        Debug.Log("[Phone] Mengambil foto!");

        // PLAY CAMERA SHUTTER SOUND! (BARU!)
        if (audioSource != null && cameraShutterSound != null)
        {
            audioSource.PlayOneShot(cameraShutterSound);
            Debug.Log("[Phone] Camera shutter sound played!");
        }

        if (photoFlash != null)
        {
            StartCoroutine(FlashEffect());
        }

        GameObject creditCard = itemHolder.GetHeldObject("CreditCard");

        if (creditCard != null)
        {
            Debug.Log("[Phone] Kartu kredit berhasil difoto!");

            if (questManager != null && !questManager.IsObjectiveComplete(questIndex, objectiveIndex))
            {
                questManager.CompleteCurrentObjective();
                Debug.Log("Objective 3 complete! Quest selesai!");
            }

            ClosePhone();

            if (dialoguePhotoManager != null)
            {
                Invoke("TriggerPhotoDialogue", 0.5f);
            }
        }
        else
        {
            Debug.Log("[Phone] Tidak ada kartu kredit yang dipegang!");

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f))
            {
                Debug.Log($"[Phone] Hit: {hit.collider.name}");

                if (hit.collider.name.Contains("CreditCard") || hit.collider.name.Contains("Kartu"))
                {
                    Debug.Log("[Phone] Kartu kredit berhasil difoto via raycast!");

                    if (questManager != null && !questManager.IsObjectiveComplete(questIndex, objectiveIndex))
                    {
                        questManager.CompleteCurrentObjective();
                    }

                    ClosePhone();

                    if (dialoguePhotoManager != null)
                    {
                        Invoke("TriggerPhotoDialogue", 0.5f);
                    }
                }
            }
        }
    }

    void TriggerPhotoDialogue()
    {
        if (dialoguePhotoManager != null)
        {
            dialoguePhotoManager.StartPhotoDialogue();
            Debug.Log("[Phone] Dialogue triggered after photo!");
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        photoFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        photoFlash.SetActive(false);
    }

    public bool IsPhoneActive()
    {
        return phoneActive;
    }
}
