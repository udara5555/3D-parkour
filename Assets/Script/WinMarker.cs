using UnityEngine;

public class WinMarker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);

        if (other.CompareTag("LocalPlayer"))
        {
            Debug.Log("WIN COUNTED");

            FindAnyObjectByType<WinMarkerSpawner>()?.AddWin();
            Destroy(gameObject);
        }
    }


}