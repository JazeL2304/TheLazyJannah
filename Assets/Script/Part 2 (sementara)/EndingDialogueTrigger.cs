using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndingDialogueTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    public string characterName = "JANNAH";
    public float textSpeed = 0.05f;

    [Header("Ending Dialogue Lines")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    [Header("Quest Integration")]
    public QuestManager questManager;
    public int questIndex = 0;
    public int objectiveIndex = 3;

    [Header("Prerequisite - Dialogue Foto Harus Selesai")]
    public int requiredQuestIndex = 0;
    public int requiredObjectiveIndex = 2;

    [Header("🎬 DIALOGUE MANAGER")]
    public Dialogue dialogueManagerScript;

    [Header("🎮 ACT PROGRESSION")]
    public bool loadNextActAfterDialogue = true; // Toggle untuk load scene berikutnya
    public int nextAct = 2;
    public int nextDay = 30;
    public string loadingSceneName = "LoadingScene";

    private bool hasTriggered = false;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    void Start()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (dialogueManagerScript == null)
        {
            dialogueManagerScript = FindObjectOfType<Dialogue>();
        }

        Debug.Log("[EndingTrigger] Script initialized!");
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLine];
                isTyping = false;
            }
            else
            {
                currentLine++;
                if (currentLine < dialogueLines.Length)
                {
                    StartCoroutine(TypeLine());
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[EndingTrigger] OnTriggerEnter called with: " + other.gameObject.name);

        if (hasTriggered)
        {
            Debug.Log("[EndingTrigger] Already triggered!");
            return;
        }

        if (questManager != null && !questManager.IsObjectiveComplete(requiredQuestIndex, requiredObjectiveIndex))
        {
            Debug.Log("[EndingTrigger] Dialogue foto belum selesai! Objective " + requiredObjectiveIndex + " belum complete.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("[EndingTrigger] Player entered! Starting dialogue...");
            StartDialogue();
            hasTriggered = true;
        }
    }

    void StartDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = characterName;
        }

        if (dialogueLines.Length > 0)
        {
            StartCoroutine(TypeLine());
        }

        if (questManager != null && !questManager.IsObjectiveComplete(questIndex, objectiveIndex))
        {
            questManager.CompleteCurrentObjective();
        }

        Debug.Log("[EndingTrigger] Dialogue started!");
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in dialogueLines[currentLine].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // ✅ PART 1: PANGGIL FUNCTION DI DIALOGUEMANAGER BUAT MUNCULIN CANVAS!
        if (dialogueManagerScript != null)
        {
            Debug.Log("[EndingTrigger] ✅ Calling DialogueManager to show ending canvas!");
            dialogueManagerScript.ShowEndingCanvas();
        }
        else
        {
            Debug.LogError("[EndingTrigger] ❌ DialogueManager script not found!");
        }

        // ✅ PART 2: LOAD NEXT ACT (JIKA DIAKTIFKAN)
        if (loadNextActAfterDialogue)
        {
            Debug.Log($"[EndingTrigger] 🎬 Act 1 Done! Preparing to load Act {nextAct} Day {nextDay}...");
            StartCoroutine(LoadNextAct());
        }

        Debug.Log("[EndingTrigger] Dialogue finished!");
    }

    IEnumerator LoadNextAct()
    {
        // Delay sebentar biar ShowEndingCanvas kebaca dulu
        yield return new WaitForSeconds(1.5f);

        // SET PROGRESS KE ACT & DAY BERIKUTNYA
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetProgress(nextAct, nextDay);
            Debug.Log($"[EndingTrigger] ✅ Progress set to ACT {nextAct} DAY {nextDay}");
        }
        else
        {
            Debug.LogWarning("[EndingTrigger] ⚠️ GameProgressManager not found! Progress not saved.");
        }

        // LOAD LOADING SCENE
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"[EndingTrigger] 🎮 Loading scene: {loadingSceneName}");
        SceneManager.LoadScene(loadingSceneName);
    }
}
