using UnityEngine;

public class CharacterGPSMovement : MonoBehaviour
{
    [Header("Reference Point")]
    public float originLatitude;
    public float originLongitude;

    [Header("Settings")]
    public float smoothSpeed = 5f;
    public float movementThreshhold = 5f;
    private Vector3 lastValidPosition;
    public Transform characterTransform;
    
    private Vector3 targetPosition;
    private bool isInitialized = false;
    
    void Start()
    {
        if (GPSManager.Instance != null)
        {
            StartCoroutine(InitializeOrigin());
        }
    }
    
    System.Collections.IEnumerator InitializeOrigin()
    {
        yield return new WaitForSeconds(2f);
        
        originLatitude = GPSManager.Instance.latitude;
        originLongitude = GPSManager.Instance.longitude;
        lastValidPosition = Vector3.zero; // Initialize
        targetPosition = Vector3.zero;
        isInitialized = true;
        
        Debug.Log($"Origin set to: {originLatitude}, {originLongitude}");
    }
    
    void Update()
    {
        if (!isInitialized || GPSManager.Instance == null) return;

        // convert GPS to Unity position
        Vector3 gpsPosition = GPSToWorldPosition(
            GPSManager.Instance.latitude,
            GPSManager.Instance.longitude
        );

        float distanceFromLast = Vector3.Distance(gpsPosition, lastValidPosition);

        if (distanceFromLast >= movementThreshhold)
        {
            lastValidPosition = gpsPosition;
            targetPosition = new Vector3(gpsPosition.x, characterTransform.position.y, gpsPosition.z);
        }
        
        // for smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
    
    Vector3 GPSToWorldPosition(float lat, float lon)
    {
        float latDiff = lat - originLatitude;
        float lonDiff = lon - originLongitude;
        
        // 111,320 meters per degree latitude
        float metersPerDegreeLat = 111320f;
        float metersPerDegreeLon = 111320f * Mathf.Cos(originLatitude * Mathf.Deg2Rad);
        
        float z = latDiff * metersPerDegreeLat;
        float x = lonDiff * metersPerDegreeLon;
        
        return new Vector3(x, 0, z);
    }
}