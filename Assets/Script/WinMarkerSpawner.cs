using UnityEngine;
using TMPro;

public class WinMarkerSpawner : MonoBehaviour
{
    public GameObject winMarkerPrefab;
    public Transform player;

    public float markerInterval = 50f;
    public float markerYOffset = 2f;

    private float nextSpawnZ;
    private bool raceStarted = false;

    private ColyseusManager net;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
    }

    void Update()
    {
        if (net == null || player == null) return;

        // detect race start
        if (!raceStarted && net.CurrentPhase == "racing")
        {
            raceStarted = true;

            nextSpawnZ = player.position.z + markerInterval;

            SpawnMarker(nextSpawnZ); // FIRST marker
        }

        if (!raceStarted) return;

        float playerZ = player.position.z;

        // spawn next AFTER passing previous
        if (playerZ >= nextSpawnZ)
        {
            nextSpawnZ += markerInterval;
            SpawnMarker(nextSpawnZ);
        }
    }

    void SpawnMarker(float zPos)
    {
        Vector3 pos = new Vector3(0f, markerYOffset, zPos);
        GameObject marker = Instantiate(winMarkerPrefab, pos, Quaternion.identity, transform);

        var label = marker.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = "+1 WINS";
    }

    public void ResetSpawner()
    {
        raceStarted = false;
        nextSpawnZ = 0f;

        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
}