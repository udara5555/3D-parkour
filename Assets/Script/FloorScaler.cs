using UnityEngine;

public class FloorScaler : MonoBehaviour
{
    [Header("References")]
    public Transform floor;
    
    private ColyseusManager net;
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private float currentScaleZ;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
        
        // If floor not assigned, try to find it
        if (floor == null)
            floor = GetComponent<Transform>();
        
        // Store initial values
        initialScale = floor.localScale;
        initialPosition = floor.position;
        currentScaleZ = initialScale.z;
    }

    void Update()
    {
        if (net == null || !net.IsInRoom || net.CurrentPhase != "racing")
            return;

        // Calculate delta based on player's current speed
        float scaleDelta = net.LocalPlayerSpeed * Time.deltaTime;
        float positionDelta = (net.LocalPlayerSpeed * 0.5f) * Time.deltaTime; // Half the speed
        
        // Increase Z scale by full speed amount
        currentScaleZ += scaleDelta;
        floor.localScale = new Vector3(initialScale.x, initialScale.y, currentScaleZ);
        
        // Increase Z position by half the speed amount
        Vector3 newPosition = floor.position;
        newPosition.z += positionDelta;
        floor.position = newPosition;
    }

    // Optional: Reset floor to initial scale and position
    public void ResetFloorScale()
    {
        currentScaleZ = initialScale.z;
        floor.localScale = initialScale;
        floor.position = initialPosition;
    }
}