using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questUI;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;

    [Header("Quest Settings")]
    public Quest[] quests;

    [Header("Persistent Objective")]
    [Tooltip("Index objective yang selalu muncul (misal: 4 untuk objective terakhir)")]
    public int persistentObjectiveIndex = 4;
    [Tooltip("Aktifkan persistent objective?")]
    public bool showPersistentObjective = true;

    private int currentQuestIndex = 0;
    private int currentObjectiveIndex = 0;

    [System.Serializable]
    public class Quest
    {
        public string questTitle;
        public Objective[] objectives;
    }

    [System.Serializable]
    public class Objective
    {
        public string description;
        public bool isCompleted;
    }

    void Start()
    {
        if (questUI != null)
        {
            questUI.SetActive(false);
        }
    }

    public void StartQuest(int questIndex)
    {
        if (questIndex >= 0 && questIndex < quests.Length)
        {
            currentQuestIndex = questIndex;
            currentObjectiveIndex = 0;

            if (questUI != null)
            {
                questUI.SetActive(true);
            }

            UpdateQuestUI();
            Debug.Log("Quest dimulai: " + quests[currentQuestIndex].questTitle);
        }
    }

    public void CompleteCurrentObjective()
    {
        Quest currentQuest = quests[currentQuestIndex];

        if (currentObjectiveIndex < currentQuest.objectives.Length)
        {
            currentQuest.objectives[currentObjectiveIndex].isCompleted = true;
            Debug.Log("Objective selesai: " + currentQuest.objectives[currentObjectiveIndex].description);

            currentObjectiveIndex++;

            // Jika sudah sampai di objective terakhir sebelum persistent objective
            // maka complete juga persistent objective
            if (currentObjectiveIndex == persistentObjectiveIndex && showPersistentObjective)
            {
                currentQuest.objectives[persistentObjectiveIndex].isCompleted = true;
                Debug.Log("Persistent objective selesai: " + currentQuest.objectives[persistentObjectiveIndex].description);
                currentObjectiveIndex++;
            }

            if (currentObjectiveIndex >= currentQuest.objectives.Length)
            {
                CompleteQuest();
            }
            else
            {
                UpdateQuestUI();
            }
        }
    }

    void CompleteQuest()
    {
        Debug.Log("Quest selesai: " + quests[currentQuestIndex].questTitle);

        if (questUI != null)
        {
            questUI.SetActive(false);
        }
    }

    void UpdateQuestUI()
    {
        Quest currentQuest = quests[currentQuestIndex];

        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questTitle;
        }

        if (questDescriptionText != null && currentObjectiveIndex < currentQuest.objectives.Length)
        {
            string objectivesText = "";

            // Tampilkan objective saat ini
            objectivesText = "• " + currentQuest.objectives[currentObjectiveIndex].description;

            // Tambahkan persistent objective jika aktif dan bukan objective saat ini
            if (showPersistentObjective &&
                persistentObjectiveIndex < currentQuest.objectives.Length &&
                currentObjectiveIndex != persistentObjectiveIndex)
            {
                objectivesText += "\n• " + currentQuest.objectives[persistentObjectiveIndex].description;
            }

            questDescriptionText.text = objectivesText;
        }
    }

    public bool IsObjectiveComplete(int questIndex, int objectiveIndex)
    {
        if (questIndex >= 0 && questIndex < quests.Length)
        {
            Quest quest = quests[questIndex];
            if (objectiveIndex >= 0 && objectiveIndex < quest.objectives.Length)
            {
                return quest.objectives[objectiveIndex].isCompleted;
            }
        }
        return false;
    }
}