using UnityEngine;
using TMPro;

public class EventDetailsUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI dateTimeText;

    // This function runs automatically every time the panel is enabled
    void OnEnable()
    {
        // Get the event that was selected in the EventManager
        EventData selected = EventManager.Instance.selectedEvent;

        // If an event was found, update the text fields
        if (selected != null)
        {
            titleText.text = selected.eventTitle;
            locationText.text = $"📍 {selected.eventLocation}";
            dateTimeText.text = $"{selected.eventDay}\n{selected.eventTime}";
            
            // Get full description from UTDEventsSync if available
            string description = GetFullDescription(selected);
            descriptionText.text = description;
        }
    }

    // Get the full description from the UTD event data
    private string GetFullDescription(EventData eventData)
    {
        // Try to find the matching UTD event with full description
        if (UTDEventsSync.Instance != null)
        {
            foreach (var utdEvent in UTDEventsSync.Instance.currentEvents)
            {
                if (utdEvent.title == eventData.eventTitle)
                {
                    return utdEvent.description;
                }
            }
        }
        
        // Fallback if no description found
        return "No description available.";
    }

    // This function will be called by the "Back" or "Close" button
    public void CloseScreen()
    {
        gameObject.SetActive(false);
    }
}