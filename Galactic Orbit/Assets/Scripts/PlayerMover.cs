using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class PlayerMover : MonoBehaviour
{
    [Header("References")]
    public AbstractMap map;               // Assign your Mapbox map here
    public Transform playerTransform;     // Assign your player GameObject here

    [Header("Dummy GPS Settings")]
    public float moveSpeed = 5f;          // Units per second in Unity space
    public Vector2d startLocation = new Vector2d(37.7749, -122.4194); // Example: SF
    public Vector2d targetLocation = new Vector2d(37.7755, -122.4185); // Example: nearby

    private Vector3 targetWorldPos;

    void Start()
    {
        // Convert the target GPS to a world position once at start
        targetWorldPos = map.GeoToWorldPosition(targetLocation, true);

        // Spawn player at starting GPS position
        var startWorldPos = map.GeoToWorldPosition(startLocation, true);
        playerTransform.position = startWorldPos;
    }

    void Update()
    {
        // Update target position if the map moves or zooms
        targetWorldPos = map.GeoToWorldPosition(targetLocation, true);

        // Move player toward target position
        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );
    }
}