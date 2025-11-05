using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public QuestManager questManager;

    // This line is the main change. It's now a list of QuestButton scripts.
    public List<QuestButton> questButtons;

    void OnEnable()
    {
        Invoke(nameof(PullNewQuests), 0.1f);
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
                questButtons[i].gameObject.SetActive(true);
                // We call the Setup function on the button's own script
                questButtons[i].Setup(uniqueQuests[i]);
            }
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }
    }
}