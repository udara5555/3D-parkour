using UnityEngine;

// Attach this to an empty GameObject with a Box Collider (Is Trigger = true)
// Place it at the START RACE spot in your scene
public class ReadyZone : MonoBehaviour
{
    private ColyseusManager net;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Only the local player triggers this (not remote players)
        if (other.CompareTag("LocalPlayer"))
        {
            Debug.Log("Entered ready zone");
            net?.SendReady(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            Debug.Log("Left ready zone");
            net?.SendReady(false);
        }
    }
}