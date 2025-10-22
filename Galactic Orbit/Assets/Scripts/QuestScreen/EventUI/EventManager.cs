using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public List<EventData> allEvents;
    public EventData selectedEvent { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public List<EventData> GetUniqueRandomEvents(int count)
    {
        if (count >= allEvents.Count)
        {
            return new List<EventData>(allEvents);
        }

        List<EventData> availableEvents = new List<EventData>(allEvents);
        List<EventData> chosenEvents = new List<EventData>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableEvents.Count);
            chosenEvents.Add(availableEvents[randomIndex]);
            availableEvents.RemoveAt(randomIndex);
        }

        return chosenEvents;
    }

    public void SelectEvent(EventData selected)
    {
        selectedEvent = selected;
    }

}