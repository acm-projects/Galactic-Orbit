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
        questTitleText.text = $"{myQuest.questTitle}\nReward: {myQuest.rewardAmount} XP";
    }

    public void OnButtonClick()
    {
        QuestManager.Instance.SelectQuest(myQuest);
        questDetailsScreen.SetActive(true);
    }
}