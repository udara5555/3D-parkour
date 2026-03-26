using UnityEngine;

public class WallToggle : MonoBehaviour
{
    private ColyseusManager net;
    private Collider col;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (net == null) return;

        // disable wall during racing
        if (net.CurrentPhase == "racing")
        {
            if (col.enabled)
                col.enabled = false;
        }
        else
        {
            if (!col.enabled)
                col.enabled = true;
        }
    }
}