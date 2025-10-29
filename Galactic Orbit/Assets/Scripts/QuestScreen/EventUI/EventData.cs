using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Event")]
public class EventData : ScriptableObject
{
    public string eventID;
    public string eventTitle;
    public string eventDay;
    public string eventTime;
    public string eventLocation;

    [Header("Event Details")]
    [Header("GPS Location")]
    [Tooltip("GPS coordinates: x = latitude, y = longitude")]
    public Vector2 gpsCoordinates; // x = latitude, y = longitude

    [Tooltip("How close player must be to check in (in meters)")]
    public float activationRadius = 50f;

    [Header("Rewards")]
    public int rewardPoints = 100;

    [Header("Runtime State")]
    [System.NonSerialized] public bool hasAttended = false;
}