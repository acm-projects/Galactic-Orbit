using UnityEngine;
using TMPro;

public class EventButton : MonoBehaviour
{
    public TextMeshProUGUI eventTitleText;
    // public GameObject eventDetailsScreen; // Uncomment if you add a details screen

    private EventData myEvent;

    // The EventUI script will call this to set the button's info
    public void Setup(EventData eventData)
    {
        myEvent = eventData;
        eventTitleText.text = $"{myEvent.eventTitle}\n{myEvent.eventLocation} at {myEvent.eventTime}";
    }

    public void OnButtonClick()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.SelectEvent(myEvent);
        }
        // if (eventDetailsScreen != null) { eventDetailsScreen.SetActive(true); }
    }
}