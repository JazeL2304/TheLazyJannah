using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoice : MonoBehaviour
{
    [Header("Choice UI Elements")]
    public GameObject choicePanel;
    public Button choice1Button;
    public Button choice2Button;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;

    [Header("Choice Settings")]
    public string choice1Label = "Jangan curi";
    public string choice2Label = "Ambil kartu kredit";

    private GameManager gameManager;
    private bool choiceShown = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("ERROR: GameManager tidak ditemukan!");
        }

        if (choice1Text != null) choice1Text.text = choice1Label;
        if (choice2Text != null) choice2Text.text = choice2Label;

        if (choice1Button != null)
        {
            choice1Button.onClick.AddListener(OnChoice1Selected);
        }
        if (choice2Button != null)
        {
            choice2Button.onClick.AddListener(OnChoice2Selected);
        }

        HideChoicePanel();
    }

    public void ShowChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
            choiceShown = true;

            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Choice Panel ditampilkan!");
        }
        else
        {
            Debug.LogError("Choice Panel NULL! Set di Inspector!");
        }
    }

    // ✅ UBAH JADI PUBLIC agar bisa dipanggil dari luar
    public void HideChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            choiceShown = false;

            Debug.Log("Choice Panel disembunyikan!"); // ← Log tambahan

            // Jangan lock cursor di sini, biar GameManager yang atur
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
    }

    void OnChoice1Selected()
    {
        Debug.Log("=== PILIHAN 1: JANGAN CURI ===");
        HideChoicePanel(); // Sembunyikan dulu

        if (gameManager != null)
        {
            gameManager.OnPlayerChoice(1);
        }
        else
        {
            Debug.LogError("GameManager NULL!");
        }
    }

    void OnChoice2Selected()
    {
        Debug.Log("=== PILIHAN 2: AMBIL KARTU KREDIT ===");
        HideChoicePanel(); // Sembunyikan dulu

        if (gameManager != null)
        {
            gameManager.OnPlayerChoice(2);
        }
        else
        {
            Debug.LogError("GameManager NULL!");
        }
    }

    public bool IsChoiceShown()
    {
        return choiceShown;
    }
}