using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> allQuests;
    public Quest selectedQuest { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public List<Quest> GetUniqueRandomQuests(int count)
    {
        if (count >= allQuests.Count)
        {
            return new List<Quest>(allQuests);
        }

        List<Quest> availableQuests = new List<Quest>(allQuests);
        List<Quest> chosenQuests = new List<Quest>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableQuests.Count);
            chosenQuests.Add(availableQuests[randomIndex]);
            availableQuests.RemoveAt(randomIndex);
        }

        return chosenQuests;
    }
    
    public void SelectQuest(Quest quest)
    {
        selectedQuest = quest;
    }
}