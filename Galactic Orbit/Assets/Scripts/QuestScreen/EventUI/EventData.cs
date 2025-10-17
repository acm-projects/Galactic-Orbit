using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Event")]
public class EventData : ScriptableObject
{
    public string eventTitle;
    public string eventDay;
    public string eventTime;
    public string eventLocation;
}