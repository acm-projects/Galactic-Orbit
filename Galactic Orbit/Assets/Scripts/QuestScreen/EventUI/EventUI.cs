using System.Collections.Generic;
using UnityEngine;

public class EventUI : MonoBehaviour
{
    public EventManager eventManager;
    public List<EventButton> eventButtons;

    void OnEnable()
    {
        PullNewEvents();
    }

    public void PullNewEvents()
    {
        List<EventData> uniqueEvents = eventManager.GetUniqueRandomEvents(eventButtons.Count);

        for (int i = 0; i < eventButtons.Count; i++)
        {
            if (i < uniqueEvents.Count)
            {
                eventButtons[i].gameObject.SetActive(true);
                eventButtons[i].Setup(uniqueEvents[i]);
            }
            else
            {
                eventButtons[i].gameObject.SetActive(false);
            }
        }
    }
}