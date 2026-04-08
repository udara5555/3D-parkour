using UnityEngine;

public class FloorScaler : MonoBehaviour
{
    [Header("References")]
    public Transform floor;
    
    private ColyseusManager net;
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private float currentScaleZ;
    private int localClickCount = 0;
    private int frameDelayCounter = 0;
    private const int RESET_FRAME_DELAY = 10;
    private bool pendingReset = false;

    Renderer rend;

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

        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (net == null || !net.IsInRoom)
            return;

        // Handle pending reset with frame delay
        if (pendingReset)
        {
            frameDelayCounter++;
            if (frameDelayCounter >= RESET_FRAME_DELAY)
            {
                ResetFloorScale();
                pendingReset = false;
                frameDelayCounter = 0;
            }
            return;
        }

        // Trigger reset when racing phase ends
        if (net.CurrentPhase != "racing")
        {
            pendingReset = true;
            frameDelayCounter = 0;
            return;
        }

        // Get local player reference to sync click count
        GameObject localPlayer = GameObject.FindWithTag("LocalPlayer");
        if (localPlayer != null)
        {
            PlayerMovementNew playerMovement = localPlayer.GetComponent<PlayerMovementNew>();
            if (playerMovement != null)
            {
                // Use reflection to get the private localClickCount
                var field = typeof(PlayerMovementNew).GetField("localClickCount", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    localClickCount = (int)field.GetValue(playerMovement);
            }
        }

        // Calculate final speed with click multiplier (same as player)
        float clickMultiplier = 1f + (localClickCount * 0.1f); // Each click adds 10% speed
        float finalSpeed = net.LocalPlayerSpeed * clickMultiplier;
        
        // Calculate scale delta based on player's actual speed with click multiplier
        float scaleDelta = finalSpeed * Time.deltaTime;
        
        // Increase Z scale by full speed amount
        currentScaleZ += scaleDelta;
        floor.localScale = new Vector3(initialScale.x, initialScale.y, currentScaleZ);
        
        // Move floor forward by half the scale increase
        // This makes the right end stay constant while left end grows
        Vector3 newPosition = floor.position;
        newPosition.z += scaleDelta * 0.5f;
        floor.position = newPosition;

        // Update texture scale to match world scale
        Vector3 scale = transform.localScale;
        rend.material.mainTextureScale = new Vector2(scale.x, scale.z);
    }

    // Reset floor to initial scale and position
    public void ResetFloorScale()
    {
        currentScaleZ = initialScale.z;
        floor.localScale = initialScale;
        floor.position = initialPosition;
    }
}