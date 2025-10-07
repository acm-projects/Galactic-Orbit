using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<Quest> allQuests;

    // This is the new function to get a list of unique quests.
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
            Quest chosenQuest = availableQuests[randomIndex];

            chosenQuests.Add(chosenQuest);
            availableQuests.RemoveAt(randomIndex);
        }

        return chosenQuests;
    }

    public Quest GetRandomQuest()
    {
        if (allQuests == null || allQuests.Count == 0)
        {
            Debug.LogError("The quest list is empty!");
            return null;
        }
        int randomIndex = Random.Range(0, allQuests.Count);
        return allQuests[randomIndex];
    }
}