using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using System;
using Mapbox.Utils;
public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance
    {
        get;
        private set;
    }

    public float latitude;
    public float longitude;
    public bool HasGPS = true;
    private const double EarthRadius = 3959; // miles

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        
        if (!Input.location.isEnabledByUser)
        {
            HasGPS = false;
            Debug.Log("GPS is not enabled on device.");
            yield break;
        }

        // Start service
        Input.location.Start(1f, 1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        InvokeRepeating("UpdateGPSData", 0f, 1f);
    }

    void UpdateGPSData()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
        }
    }

    public double GetMilesDistanceFromLocation(Vector2d location)
    {
        double distance = HaversineDistance(
            location.x, location.y,
            latitude, longitude
        );
        return distance;
    }
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = Deg2Rad(lat2 - lat1);
        double dLon = Deg2Rad(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c;
    }
    private double Deg2Rad(double deg)
    {
        return deg * Math.PI / 180.0;
    }
}
