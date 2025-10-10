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
            questDescriptionText.text = "• " + currentQuest.objectives[currentObjectiveIndex].description;
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