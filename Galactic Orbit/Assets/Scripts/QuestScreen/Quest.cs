using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class Quest : ScriptableObject
{
    public string questTitle;
    [TextArea(3, 10)]
    public string questDescription;
    public int rewardAmount;
}