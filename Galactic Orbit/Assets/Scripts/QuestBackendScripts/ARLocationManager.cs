using UnityEngine;
using System.Collections;

public class ARLocationManager : MonoBehaviour
{
    public float completionRadius = 20f; // meters

    void Start()
    {
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location not enabled.");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.Log("Location service failed.");
            yield break;
        }

        Debug.Log("Location service started.");
        InvokeRepeating(nameof(CheckQuestProximity), 2f, 5f);
    }

    void CheckQuestProximity()
    {
        if (QuestManager.Instance == null || Input.location.status != LocationServiceStatus.Running) return;

        Vector2 playerPos = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);

        foreach (Quest quest in QuestManager.Instance.activeQuests)
        {
            if (!quest.isCompleted)
            {
                float distance = GetDistanceMeters(playerPos, quest.targetLocation);
                if (distance < completionRadius)
                {
                    QuestManager.Instance.CompleteQuest(quest.questID);
                }
            }
        }
    }

    float GetDistanceMeters(Vector2 a, Vector2 b)
    {
        var R = 6371000f; // radius of Earth in meters
        var dLat = Mathf.Deg2Rad * (b.x - a.x);
        var dLon = Mathf.Deg2Rad * (b.y - a.y);
        var lat1 = Mathf.Deg2Rad * a.x;
        var lat2 = Mathf.Deg2Rad * b.x;

        var h = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                Mathf.Cos(lat1) * Mathf.Cos(lat2) *
                Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        return R * 2 * Mathf.Atan2(Mathf.Sqrt(h), Mathf.Sqrt(1 - h));
    }
}
