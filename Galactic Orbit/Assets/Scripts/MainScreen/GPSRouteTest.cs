using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class GPSRouteTest : MonoBehaviour
{
    public static GPSRouteTest Instance { get; private set; }   // 👈 Singleton reference

    public Vector2d[] route = {
        new Vector2d(32.98511756, -96.74944336),
        new Vector2d(32.98691499, -96.74762272),
        new Vector2d(32.98668367, -96.74952794),
    };

    public Vector2d currentLocation;
    public float latitude;
    public float longitude;
    private float lastUpdateTime;

    private int index = 0;

    void Awake()
    {
        // 👇 Enforce only one instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // optional if you want it to persist between scenes
        }
        else
        {
            Destroy(gameObject); // avoid duplicates
        }
    }

    void Start()
    {
        lastUpdateTime = Time.time;
        currentLocation = route[index];
        latitude = (float)currentLocation.x;
        longitude = (float)currentLocation.y;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= 2f) // every 2 seconds
        {
            lastUpdateTime = Time.time;
            index = (index + 1) % route.Length;
            currentLocation = route[index];
            latitude = (float)currentLocation.x;
            longitude = (float)currentLocation.y;
        }
    }
}
