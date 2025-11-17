using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public QuestManager questManager;

    // This line is the main change. It's now a list of QuestButton scripts.
    public List<QuestButton> questButtons;

    void Start()
    {
        StartCoroutine(WaitForQuestManager());
    }
    void OnEnable()
    {
        ReloadQuests();
    }
    // Reloading so completed quests get removed
    private void ReloadQuests()
    {
        for (int i = 0; i < questButtons.Count; i++)
        {
            if (i < questManager.activeQuests.Count)
            {
                if (questManager.activeQuests[i].isCompleted == true)
                    continue;

                questButtons[i].gameObject.SetActive(true);
                // We call the Setup function on the button's own script
                Quest activeQuest = questManager.activeQuests[i];
                questManager.ActivateQuest(activeQuest);
                questButtons[i].Setup(activeQuest);
            }
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private IEnumerator WaitForQuestManager()
    {
        // Wait until the singleton is available
        while (QuestManager.Instance == null)
            yield return null; // wait one frame

        questManager = QuestManager.Instance;

        PullNewQuests(); // now safe
    }

    void Awake()
    {
        if (questManager == null)
            questManager = QuestManager.Instance;
    }

    public void PullNewQuests()
    {
        if (questManager == null)
        {
            Debug.LogError("QuestManager reference is not set in QuestUI");
            return;
        }

        if (questButtons == null || questButtons.Count == 0)
        {
            Debug.LogError("QuestButtons list not set in QuestUI");
            return;
        }

        List<Quest> uniqueQuests = questManager.GetUniqueRandomQuests(questButtons.Count);

        // This loop now gives each button its quest data
        for (int i = 0; i < questButtons.Count; i++)
        {
            if (i < uniqueQuests.Count)
            {
                if (uniqueQuests[i].isCompleted == true)
                    continue;

                questButtons[i].gameObject.SetActive(true);
                // We call the Setup function on the button's own script
                Quest activeQuest = uniqueQuests[i];
                questManager.ActivateQuest(activeQuest);
                questButtons[i].Setup(activeQuest);
            }
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }

    }
}