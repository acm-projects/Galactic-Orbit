using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public List<Event> allEvents;
    public Event selectedEvent { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public List<Event> GetUniqueRandomEvents(int count)
    {
        if (count >= allEvents.Count)
        {
            return new List<Event>(allEvents);
        }

        List<Event> availableEvents = new List<Event>(allEvents);
        List<Event> chosenEvents = new List<Event>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableEvents.Count);
            chosenEvents.Add(availableEvents[randomIndex]);
            availableEvents.RemoveAt(randomIndex);
        }

        return chosenEvents;
    }
    
    public void SelectEvent(Event selected)
    {
        selectedEvent = selected;
    }
}