using UnityEngine;
using TMPro;

// Attach this to any GameObject in the scene (e.g. an empty "MarkerManager")
// Assign the winMarkerPrefab in the Inspector
public class WinMarkerSpawner : MonoBehaviour
{
    [Header("Settings")]
    public int totalMarkers = 20;        // how many markers to spawn (20 = up to 1000 units)
    public float markerInterval = 50f;   // every 50 units on Z axis
    public float markerYOffset = 2f;     // height above floor
    public float markerXOffset = 0f;     // side offset if needed

    [Header("Prefab")]
    public GameObject winMarkerPrefab;   // assign in Inspector (see setup below)

    void Start()
    {
        SpawnMarkers();
    }

    void SpawnMarkers()
    {
        for (int i = 1; i <= totalMarkers; i++)
        {
            float zPos = i * markerInterval;

            Vector3 pos = new Vector3(markerXOffset, markerYOffset, zPos);
            GameObject marker = Instantiate(winMarkerPrefab, pos, Quaternion.identity);

            // set the label text
            var label = marker.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = (i * (int)markerInterval) + "\nWins";

            marker.name = "WinMarker_" + (i * (int)markerInterval);
        }
    }
}