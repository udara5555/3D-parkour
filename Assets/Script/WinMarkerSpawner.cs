using TMPro;
using UnityEngine;

public class WinMarkerSpawner : MonoBehaviour
{
    public GameObject winMarkerPrefab;
    public Transform player;

    public float markerInterval = 50f;
    public float markerYOffset = 2f;

    public AudioClip winSound;
    private AudioSource audioSource;

    private float nextSpawnZ;
    private bool raceStarted = false;

    private ColyseusManager net;

    public int winCount = 0;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
        
        // add AudioSource on this GameObject automatically
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = winSound;
    }

    void Update()
    {
        if (net == null || player == null) return;

        // detect race start
        if (!raceStarted && net.CurrentPhase == "racing")
        {
            raceStarted = true;

            nextSpawnZ = player.position.z + markerInterval;

            SpawnMarker(nextSpawnZ); // first marker
        }

        if (!raceStarted) return;

        float playerZ = player.position.z;

        // IMPORTANT: use WHILE (not IF)
        while (playerZ >= nextSpawnZ)
        {
            AddWin(); // count win

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

    public void AddWin()
    {
        winCount++;
        Debug.Log("Wins: " + winCount);
        PlayWinSound();
    }

    public void PlayWinSound()
    {
        if (audioSource != null && winSound != null)
            audioSource.Play();
    }

    public int GetWins()
    {
        return winCount;
    }
}