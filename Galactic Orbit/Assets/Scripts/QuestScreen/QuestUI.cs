using System.Collections.Generic; 
using UnityEngine;
using TMPro; 

public class QuestUI : MonoBehaviour
{
    
    public QuestManager questManager;
    
    
    public List<TextMeshProUGUI> questDisplaySlots;

    // This runs when the object becomes active, which is a good time to load quests.
    void OnEnable()
    {
        PullNewQuests();
    }

    public void PullNewQuests()
    {
        if (questDisplaySlots.Count == 0)
        {
            Debug.LogError("No UI slots assigned in the QuestUI script!");
            return;
        }

       
        List<Quest> uniqueQuests = questManager.GetUniqueRandomQuests(questDisplaySlots.Count);

        // Loop through each UI slot to display a quest.
        for (int i = 0; i < questDisplaySlots.Count; i++)
        {
            // If we have a quest for this slot, display it.
            if (i < uniqueQuests.Count)
            {
                questDisplaySlots[i].text = $"{uniqueQuests[i].questTitle}\nReward: {uniqueQuests[i].rewardAmount} XP";
                questDisplaySlots[i].gameObject.SetActive(true); 
            }
            else
            {
                // If the manager returns fewer quests than we have slots,
                // hide the extra slots.
                questDisplaySlots[i].gameObject.SetActive(false);
            }
        }
    }
}