using UnityEngine;

public class DanceManager : MonoBehaviour
{
    [Header("Dance Settings")]
    [SerializeField] private float idleDurationThreshold = 8f; // Trigger dance after 8 seconds of idle

    private Animator animator;
    private CharacterController cc;
    private PlayerMovementNew playerMovement;

    // Animation parameter hashes
    private int danceHash = Animator.StringToHash("Dance");
    private int walkHash = Animator.StringToHash("Walk");
    private int runHash = Animator.StringToHash("Run");
    private int jumpHash = Animator.StringToHash("Jump");
    private int sitHash = Animator.StringToHash("Sit");

    // Idle tracking
    private float idleTimer = 0f;
    private bool isDancing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovementNew>();

        if (animator == null)
        {
            Debug.LogError("DanceManager: Animator not found!");
            return;
        }

        if (cc == null)
        {
            Debug.LogError("DanceManager: CharacterController not found!");
            return;
        }
    }

    void Update()
    {
        // Check if player is idle (no movement, no jump, not sitting)
        bool isPlayerIdle = IsPlayerIdle();

        if (isPlayerIdle)
        {
            // Increment idle timer
            idleTimer += Time.deltaTime;

            // Trigger dance after 8 seconds of idle (only if not already dancing)
            if (idleTimer >= idleDurationThreshold && !isDancing)
            {
                StartDance();
            }
        }
        else
        {
            // Player is moving/jumping/sitting - stop dance and reset timer
            if (isDancing)
            {
                StopDance();
            }
            idleTimer = 0f;
        }
    }

    /// <summary>
    /// Detects if player is in idle state:
    /// - No movement input (velocity near zero)
    /// - Not jumping
    /// - Not sitting
    /// </summary>
    private bool IsPlayerIdle()
    {
        // Check if character is grounded and has minimal velocity
        if (!cc.isGrounded)
            return false;

        // Check if any movement animation is playing
        if (animator.GetBool(walkHash) || animator.GetBool(runHash))
            return false;

        // Check if jumping or sitting
        if (animator.GetBool(sitHash))
            return false;

        // Check for player input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // If joystick exists, check joystick input
        if (playerMovement != null && playerMovement.joystick != null)
        {
            h += playerMovement.joystick.Horizontal;
            v += playerMovement.joystick.Vertical;
        }

        // If there's any input, player is not idle
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            return false;

        // Check for jump or sit input
        if (Input.GetKeyDown(KeyCode.Space))
            return false;

        if (Input.GetKey(KeyCode.C))
            return false;

        return true;
    }

    private void StartDance()
    {
        isDancing = true;
        animator.SetBool(danceHash, true);
        Debug.Log("Dance started! Idle duration: " + idleTimer);
    }

    private void StopDance()
    {
        isDancing = false;
        animator.SetBool(danceHash, false);
        Debug.Log("Dance stopped - player input detected");
    }
}