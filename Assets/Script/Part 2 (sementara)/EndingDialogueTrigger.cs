using UnityEngine;
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
    public int requiredObjectiveIndex = 2; // ← Objective "Balik ke kamar"

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

        // CEK: Dialogue foto sudah selesai belum?
        if (questManager != null && !questManager.IsObjectiveComplete(requiredQuestIndex, requiredObjectiveIndex))
        {
            Debug.Log("[EndingTrigger] Dialogue foto belum selesai! Objective " + requiredObjectiveIndex + " belum complete.");
            return; // ← BLOKIR trigger!
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

        Debug.Log("[EndingTrigger] Dialogue finished!");
    }
}
