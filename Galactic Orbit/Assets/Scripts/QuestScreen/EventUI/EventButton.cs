using UnityEngine;
using TMPro;

public class EventButton : MonoBehaviour
{
    public TextMeshProUGUI eventTitleText;
    public GameObject eventDetailsScreen;

    private EventData myEvent;

    // The EventUI script will call this to set the button's info
    public void Setup(EventData eventData)
    {
        myEvent = eventData;
        eventTitleText.text = $"{myEvent.eventTitle}\n{myEvent.eventDay}, {myEvent.eventTime}\n📍 {myEvent.eventLocation}";
    }

    public void OnButtonClick()
    {

        EventManager.Instance.SelectEvent(myEvent);
        eventDetailsScreen.SetActive(true);


    }
}