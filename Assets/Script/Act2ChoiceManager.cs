using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Act2ChoiceManager : MonoBehaviour
{
    [Header("Choice UI Elements")]
    public GameObject choicePanel;
    public Button choice1Button;
    public Button choice2Button;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;

    [Header("Choice Settings")]
    public string choice1Label = "Mengaku dan minta maaf";
    public string choice2Label = "Diam dan pura-pura tidak tahu";

    [Header("Audio")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    private bool choiceShown = false;

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set text labels
        if (choice1Text != null) choice1Text.text = choice1Label;
        if (choice2Text != null) choice2Text.text = choice2Label;

        // Add button listeners
        if (choice1Button != null)
        {
            choice1Button.onClick.AddListener(OnChoice1Selected);
        }
        if (choice2Button != null)
        {
            choice2Button.onClick.AddListener(OnChoice2Selected);
        }

        // Hide panel at start
        HideChoicePanel();
    }

    public void ShowChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
            choiceShown = true;

            // Unlock cursor untuk klik button
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("[Act2Choice] Choice Panel ditampilkan!");
        }
        else
        {
            Debug.LogError("[Act2Choice] Choice Panel NULL! Set di Inspector!");
        }
    }

    public void HideChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            choiceShown = false;

            Debug.Log("[Act2Choice] Choice Panel disembunyikan!");
        }
    }

    void OnChoice1Selected()
    {
        Debug.Log("[Act2Choice] ===== PILIHAN 1: MENGAKU =====");

        PlayClickSound();
        HideChoicePanel();

        // TODO: Implement logic untuk ending "Mengaku"
        // Contoh: Load scene ending "Jujur"
        Debug.Log("[Act2Choice] Player memilih untuk MENGAKU dan MINTA MAAF");

        // Placeholder - Anda bisa ganti dengan scene/logic sendiri
        // SceneManager.LoadScene("Act2_EndingJujur");
    }

    void OnChoice2Selected()
    {
        Debug.Log("[Act2Choice] ===== PILIHAN 2: DIAM =====");

        PlayClickSound();
        HideChoicePanel();

        // TODO: Implement logic untuk ending "Diam"
        // Contoh: Load scene ending "Bohong"
        Debug.Log("[Act2Choice] Player memilih untuk DIAM dan PURA-PURA TIDAK TAHU");

        // Placeholder - Anda bisa ganti dengan scene/logic sendiri
        // SceneManager.LoadScene("Act2_EndingBohong");
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public bool IsChoiceShown()
    {
        return choiceShown;
    }
}