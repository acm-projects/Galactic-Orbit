using UnityEngine;
using TMPro;

public class EventButton : MonoBehaviour
{
    //public TextMeshProUGUI eventTitleText;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI DateText;
    //public TextMeshProUGUI LocationText;
    public GameObject eventDetailsScreen;

    private EventData myEvent;

    // The EventUI script will call this to set the button's info
    public void Setup(EventData eventData)
    {
        myEvent = eventData;
        Debug.Log("Titles:" + myEvent.eventTitle);
        //eventTitleText.text = $"{myEvent.eventTitle}\n{myEvent.eventDay}, {myEvent.eventTime}\n📍 {myEvent.eventLocation}";
        string totalEventTitle = myEvent.eventTitle;

        int first = totalEventTitle.IndexOf(' ');
        int second = totalEventTitle.IndexOf(' ', first + 1);

        string title = totalEventTitle.Substring(0, second);
        string desc = totalEventTitle.Substring(second + 1);

        TitleText.text = title;
        DescriptionText.text = desc;

        string dayText = myEvent.eventDay.Substring(myEvent.eventDay.IndexOf(", ") + 1);
        string month = dayText.Substring(1, dayText.Substring(1).IndexOf(" "));
        month = AbbreviateMonth(month);
        string day = dayText.Substring(dayText.Substring(1).IndexOf(" ") + 1);

        

        string dateText = $"{month} {day}, {myEvent.eventTime}";
        DateText.text = dateText;

        //LocationText.text = myEvent.eventLocation;
    }
    private string AbbreviateMonth(string month)
    {
        Debug.Log("COOL" + month + "|");
        if (month.Equals("January")) 
            month = "Jan.";
        else if (month.Equals("February")) 
            month = "Feb.";
        else if (month.Equals("March")) 
            month = "Mar.";
        else if (month.Equals("April")) 
            month = "Apr.";
        else if (month.Equals("May")) 
            month = "May";
        else if (month.Equals("June")) 
            month = "June";
        else if (month.Equals("July")) 
            month = "July";
        else if (month.Equals("August")) 
            month = "Aug.";
        else if (month.Equals("Spetember")) 
            month = "Sep.";
        else if (month.Equals("October")) 
            month = "Oct.";
        else if (month.Equals("November")) 
            month = "Nov.";
        else if (month.Equals("December")) 
            month = "Dec.";
        return month;
    }

    public void OnButtonClick()
    {

        EventManager.Instance.SelectEvent(myEvent);
        eventDetailsScreen.SetActive(true);


    }
}