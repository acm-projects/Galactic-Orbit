using UnityEngine;
using TMPro;
using System;

public class QuestButton : MonoBehaviour
{
    //public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI RewardText;


    public GameObject questDetailsScreen;
    public Quest myQuest;

    public void Setup(Quest quest)
    {
        myQuest = quest;
        //questTitleText.text = $"{myQuest.questTitle}\nReward: {myQuest.rewardPoints} XP";
        string entireText = myQuest.questTitle;
        int indexOfSeparation = entireText.IndexOf(" ");

        if (indexOfSeparation < 0)
        {
            TitleText.text = "";
            DescriptionText.text = entireText;
        }
        else
        {
            TitleText.text = entireText.Substring(0, indexOfSeparation);
            DescriptionText.text = entireText.Substring(indexOfSeparation + 1);
        }
        RewardText.text = $"{myQuest.rewardPoints} XP";
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