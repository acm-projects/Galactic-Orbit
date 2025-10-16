using UnityEngine;
using TMPro;

public class GPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI latitudeText;
    public TextMeshProUGUI longitudeText;
    public TextMeshProUGUI debugConsole;

    void Update()
    {
        //debugConsole.text = GameManager.Instance.value
        if (GPSManager.Instance != null)
        {
            latitudeText.text = "Latitude: " + GPSManager.Instance.latitude.ToString("F6");
            longitudeText.text = "Longitude: " + GPSManager.Instance.longitude.ToString("F6");
        }
        else
        {
            latitudeText.text = "Latitude: " + null;
            longitudeText.text = "Longitude: " + null;
        }
        debugConsole.text = "GPS Enabled: " + GPSManager.Instance.HasGPS.ToString();
    }
}
