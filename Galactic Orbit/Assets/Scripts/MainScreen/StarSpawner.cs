using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using System.IO;

public class StarSpawner : MonoBehaviour
{
    [SerializeField] private AbstractMap map;
    [SerializeField] private GameObject buildingPrefab;

    private List<GameObject> spawnedBuildings = new List<GameObject>();
    private List<Vector2d> buildingCoordinates = new List<Vector2d>();
    private List<string> buildingNames = new List<string>();

    void Start()
    {
        LoadCSV();
        map.OnInitialized += OnMapLoaded;
        map.OnUpdated += OnMapLoaded; // Called on map movement/zoom/recenter
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
        float heightOffset = 5f;

        for (int i = 0; i < buildingCoordinates.Count; i++)
        {
            Vector3 worldPos = Conversions.GeoToWorldPosition(
                buildingCoordinates[i],
                map.CenterMercator,
                map.WorldRelativeScale).ToVector3xz();

            worldPos.y += heightOffset;

            // --- If building exists, move it ---
            if (i < spawnedBuildings.Count && spawnedBuildings[i] != null)
            {
                spawnedBuildings[i].transform.position = worldPos;
            }
            // --- Otherwise instantiate new one ---
            else
            {
                GameObject go = Instantiate(buildingPrefab, worldPos, Quaternion.identity, this.transform);
                go.name = buildingNames[i];
                spawnedBuildings.Add(go);
            }
        }
    }
}
