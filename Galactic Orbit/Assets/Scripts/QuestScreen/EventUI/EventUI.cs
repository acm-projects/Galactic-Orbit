using System;
using System.Collections.Generic;
//using Mono.Cecil;
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
        Debug.Log("🔄 PullNewEvents called!");
        Debug.Log($"EventManager: {eventManager != null}");
        Debug.Log($"EventButtons count: {eventButtons?.Count}");
        Debug.Log($"Events in manager: {eventManager?.allEvents?.Count}");
        
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