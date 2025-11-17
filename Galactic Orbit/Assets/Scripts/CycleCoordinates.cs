using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleCoordinates : MonoBehaviour
{
    public static CycleCoordinates Instance { get; private set; }

    [Header("Locations (x = latitude, y = longitude)")]
    public List<Vector2> coordinates = new List<Vector2>();

    [Header("Output Values")]
    public float latitude;
    public float longitude;

    [Header("Settings")]
    public float delaySeconds = 2f;

    private int currentIndex = 0;
    private Coroutine cycleRoutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (coordinates.Count > 0)
            cycleRoutine = StartCoroutine(Cycle());
    }

    private IEnumerator Cycle()
    {
        while (true)
        {
            Vector2 pos = coordinates[currentIndex];

            // Set values
            latitude = pos.x;
            longitude = pos.y;
            GPSManager.Instance.latitude = latitude;
            GPSManager.Instance.longitude = longitude;

            // Next index
            currentIndex++;
            if (currentIndex >= coordinates.Count)
                currentIndex = 0;

            yield return new WaitForSeconds(delaySeconds);
        }
    }
}
