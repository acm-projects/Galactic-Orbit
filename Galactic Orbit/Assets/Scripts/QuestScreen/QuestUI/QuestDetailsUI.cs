using UnityEngine;
using TMPro;

public class QuestDetailsUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI rewardText;

    // This function runs automatically every time the panel is enabled
    void OnEnable()
    {
        // Get the quest that was selected in the QuestManager
        Quest selected = QuestManager.Instance.selectedQuest;

        // If a quest was found, update the text fields
        if (selected != null)
        {
            titleText.text = selected.questTitle;
            descriptionText.text = selected.questDescription;
            rewardText.text = $"{selected.rewardPoints} XP";
        }
    }

    // This function will be called by the "Back" button
    public void CloseScreen()
    {
        gameObject.SetActive(false);
    }
}