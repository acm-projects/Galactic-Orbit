using UnityEngine;
using TMPro;

public class QuestButton : MonoBehaviour
{
    public TextMeshProUGUI questTitleText;
    public GameObject questDetailsScreen;
    private Quest myQuest;

    public void Setup(Quest quest)
    {
        myQuest = quest;
        questTitleText.text = $"{myQuest.questTitle}\nReward: {myQuest.rewardPoints} XP";
    }

    public void OnButtonClick()
    {
        QuestManager.Instance.SelectQuest(myQuest);
        questDetailsScreen.SetActive(true);
    }

    // Call when player accepts/starts the quest
    public void OnAcceptQuest()
    {
        QuestManager.Instance.ActivateQuest(myQuest);
        // Close the details screen
        if (questDetailsScreen != null)
        {
            questDetailsScreen.SetActive(false);
        }
    }
}