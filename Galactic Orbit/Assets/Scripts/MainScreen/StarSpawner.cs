using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using System.IO;

public class StarSpawner : MonoBehaviour
{
    [SerializeField] private AbstractMap map;
    [SerializeField] private GameObject buildingPrefab;  // Assign your 3D model prefab here

    private List<GameObject> spawnedBuildings = new List<GameObject>();
    private List<Vector2d> buildingCoordinates = new List<Vector2d>();
    private List<string> buildingNames = new List<string>();

    void Start()
    {
        LoadCSV();
        map.OnInitialized += OnMapLoaded;
        map.OnUpdated += OnMapLoaded; // Also update positions if map recenters
    }

    void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("BuildingLocations");
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        using (StringReader reader = new StringReader(csvFile.text))
        {
            string line;
            bool isHeader = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader) { isHeader = false; continue; }

                string[] values = line.Split(',');
                if (values.Length < 3) continue;

                string name = values[0].Trim();
                double lat = double.Parse(values[1]);
                double lon = double.Parse(values[2]);

                buildingNames.Add(name);
                buildingCoordinates.Add(new Vector2d(lat, lon));
            }
        }
    }

    void OnMapLoaded()
    {
        // Destroy any previously spawned buildings if needed (e.g. after recenter)
        foreach (var b in spawnedBuildings)
            Destroy(b);
        spawnedBuildings.Clear();

        for (int i = 0; i < buildingCoordinates.Count; i++)
        {
            Vector3 worldPos = Conversions.GeoToWorldPosition(
                buildingCoordinates[i],
                map.CenterMercator,
                map.WorldRelativeScale).ToVector3xz();

            GameObject go = Instantiate(buildingPrefab, worldPos, Quaternion.identity, this.transform);
            go.name = buildingNames[i];
            spawnedBuildings.Add(go);
        }
    }
}
