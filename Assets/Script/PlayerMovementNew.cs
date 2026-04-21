using UnityEngine;

public class PlayerMovementNew : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    [Header("References")]
    public FixedJoystick joystick;
    public Transform characterModel;

    private CharacterController cc;
    private Animator animator;
    private float yaw;
    private float yVelocity = 0f;
    private bool jumpPressed = false;
    private bool sitPressed = false;
    
    // Network references
    private ColyseusManager net;
    private CharacterSwitcherNew skinSwitcher;
    private float sendTimer = 0f;
    private const float SEND_INTERVAL = 0.05f;

    // Animation parameter hashes for performance
    private int walkHash = Animator.StringToHash("Walk");
    private int runHash = Animator.StringToHash("Run");
    private int jumpHash = Animator.StringToHash("Jump");
    private int sitHash = Animator.StringToHash("Sit");
    private int dieHash = Animator.StringToHash("Die");

    // Click counting during countdown
    public int localClickCount = 0;
    public int startClicks = 0;
    private bool isAutoMoving = false;
    private float lastRecordedSpeed = 0f;
    private bool hasDiedAtEnd = false;
    private float dieAnimationTimer = 0f;
    private const float DIE_ANIMATION_DELAY = 1f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        net = FindAnyObjectByType<ColyseusManager>();
        skinSwitcher = GetComponent<CharacterSwitcherNew>();
        
        if (cc == null)
        {
            Debug.LogError("CharacterController not found! Please add a CharacterController to this object.");
            return;
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator not found! Animations will not play.");
            return;
        }

        // If characterModel not assigned, try to find it as first child
        if (characterModel == null)
            characterModel = transform.GetChild(0);

        // CRITICAL: Disable root motion so CharacterController handles movement
        animator.applyRootMotion = false;
    }

    void Update()
    {
        // FREEZE during countdown but allow clicking
        bool isFrozen = (net != null && net.CurrentPhase == "countdown");

        if (isFrozen)
        {
            HandleCountdownPhase();
            return; // Skip all other movement
        }

        // Handle die animation delay period - keep die animation playing for 1 second
        if (hasDiedAtEnd && net != null && net.CurrentPhase == "waiting")
        {
            dieAnimationTimer += Time.deltaTime;
            
            // Keep die animation parameter active during delay
            if (animator != null)
            {
                animator.SetBool(dieHash, true);
            }
            
            // After delay, transition to idle
            if (dieAnimationTimer >= DIE_ANIMATION_DELAY)
            {
                dieAnimationTimer = 0f;
                isAutoMoving = false;
                hasDiedAtEnd = false;
                if (animator != null)
                {
                    // Disable die animation to transition back to idle
                    animator.SetBool(dieHash, false);
                    animator.SetBool(walkHash, false);
                    animator.SetBool(runHash, false);
                    animator.SetBool(sitHash, false);
                    animator.ResetTrigger(jumpHash);
                }
                Debug.Log("Die animation complete, returning to idle");
            }
            // During delay, don't process any other movement
            return;
        }

        // Reset animations and auto-movement when returning to waiting phase (only if not in die animation state)
        if (net != null && net.CurrentPhase == "waiting" && !hasDiedAtEnd)
        {
            if (isAutoMoving)
            {
                isAutoMoving = false;
                dieAnimationTimer = 0f;
                if (animator != null)
                {
                    animator.SetBool(walkHash, false);
                    animator.SetBool(runHash, false);
                    animator.SetBool(sitHash, false);
                    animator.ResetTrigger(jumpHash);
                    animator.SetBool(dieHash, false);
                }
            }

            int bestEgg = GetBestEggValue();
            startClicks = bestEgg / 10;
            localClickCount = 0;
        }

        // Check for speed change during racing phase
        if (net != null && net.CurrentPhase == "racing")
        {
            if (net.LocalPlayerSpeed != lastRecordedSpeed)
            {
                isAutoMoving = true;
                hasDiedAtEnd = false;
                dieAnimationTimer = 0f;
                lastRecordedSpeed = net.LocalPlayerSpeed;
                Debug.Log($"Speed changed to {lastRecordedSpeed}, enabling auto-movement in +Z direction with click multiplier");
            }
        }

        // Handle automatic movement during racing phase
        if (isAutoMoving && net != null && net.CurrentPhase == "racing")
        {
            HandleRacingAutoMovement();
            return; // Skip normal movement controls
        }

        HandleRotation();
        HandleJump();
        HandleMovement();
        HandleSit();
        
        // Send network updates
        if (net != null && net.IsInRoom)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= SEND_INTERVAL)
            {
                sendTimer = 0f;
                SendNetworkUpdate();
            }
        }
    }

    void HandleCountdownPhase()
    {
        // Detect click/tap during countdown
        if (Input.GetMouseButtonDown(0))
        {
            if (localClickCount == 0)
            {
                localClickCount = startClicks;
                net?.SendInitialBonus(startClicks);
            }

            localClickCount++;
            net?.SendClick();
            Debug.Log("Local Clicks: " + localClickCount + " | Server Clicks: " + net.ServerClickCount);
        }

        // Stop all animations during countdown
        if (animator != null)
        {
            animator.SetBool(walkHash, false);
            animator.SetBool(sitHash, false);
            animator.ResetTrigger(jumpHash);
        }

        // Apply gravity while frozen
        if (cc.isGrounded)
            yVelocity = -2f;
        else
            yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    void HandleRacingAutoMovement()
    {
        // Check if we've already triggered die animation (transitioning from racing to waiting)
        if (!hasDiedAtEnd && net != null && net.CurrentPhase != "racing")
        {
            hasDiedAtEnd = true;
            dieAnimationTimer = 0f;
            if (animator != null)
            {
                animator.SetBool(runHash, false);
                animator.SetTrigger(dieHash);
                Debug.Log("Die animation triggered at end of race");
            }
            return;
        }

        // Use server-assigned speed during racing phase (same technique as PlayerMovement.cs)
        float currentSpeed = speed;
        if (net != null && net.CurrentPhase == "racing")
            currentSpeed = net.LocalPlayerSpeed;

        // Auto-move in +Z direction
        Vector3 autoMove = Vector3.forward * currentSpeed * Time.deltaTime;
        cc.Move(autoMove);

        // Apply gravity
        if (cc.isGrounded)
            yVelocity = -2f;
        else
            yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);

        // Play running animation instead of walking
        if (animator != null)
        {
            animator.SetBool(walkHash, false);
            animator.SetBool(runHash, true);
            animator.SetBool(sitHash, false);
            animator.ResetTrigger(jumpHash);
        }

        // Keep character facing +Z direction - rotate around Y axis only
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        if (characterModel != null)
        {
            // Rotate character model around Y axis only to face +Z direction
            Vector3 currentEuler = characterModel.localEulerAngles;
            float targetYAngle = 0f; // Face forward (+Z)
            float smoothedYAngle = Mathf.LerpAngle(currentEuler.y, targetYAngle, Time.deltaTime * 10f);
            characterModel.localRotation = Quaternion.Euler(currentEuler.x, smoothedYAngle, currentEuler.z);
        }

        // Send network update
        if (net.IsInRoom)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= SEND_INTERVAL)
            {
                sendTimer = 0f;
                net.SendMove(transform.position, 0f, "run");
            }
        }
    }

    void HandleRotation()
    {
        // Mouse yaw (camera direction)
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleJump()
    {
        // Jump input from keyboard or button
        bool jumpInput = Input.GetKeyDown(KeyCode.Space) || jumpPressed;
        
        if (cc.isGrounded && jumpInput && !animator.GetBool(sitHash))
        {
            yVelocity = jumpForce;
            if (animator != null)
                animator.SetTrigger(jumpHash);
            Debug.Log("Jump!");
        }
        
        jumpPressed = false;
    }

    void HandleSit()
    {
        // Sit input from keyboard or button
        bool sitInput = Input.GetKey(KeyCode.C) || sitPressed;
        
        if (animator != null)
            animator.SetBool(sitHash, sitInput);
    }

    void HandleMovement()
    {
        // Don't move while sitting
        if (animator != null && animator.GetBool(sitHash))
        {
            // Apply gravity while sitting
            if (cc.isGrounded && yVelocity < 0)
                yVelocity = -2f;
            else
                yVelocity += gravity * Time.deltaTime;

            cc.Move(Vector3.up * yVelocity * Time.deltaTime);
            return;
        }


        // Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (joystick != null)
        {
            h += joystick.Horizontal;
            v += joystick.Vertical;
        }

        // Local input direction (for animation rotation)
        Vector3 localInput = new Vector3(h, 0f, v);
        
        // Rotate character model around Y axis only to face movement direction
        if (localInput.sqrMagnitude > 0.01f && characterModel != null)
        {
            // Calculate target Y rotation angle from input direction
            float targetYAngle = Mathf.Atan2(localInput.x, localInput.z) * Mathf.Rad2Deg;
            
            // Get current rotation and only modify Y
            Vector3 currentEuler = characterModel.localEulerAngles;
            float currentYAngle = currentEuler.y;
            
            // Smooth rotation around Y axis only
            float smoothedYAngle = Mathf.LerpAngle(currentYAngle, targetYAngle, Time.deltaTime * 10f);
            
            // Keep X and Z at their current values, only change Y
            characterModel.localRotation = Quaternion.Euler(currentEuler.x, smoothedYAngle, currentEuler.z);
        }

        // Movement relative to camera direction
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        Vector3 moveDir = transform.TransformDirection(inputDir);
        moveDir *= speed;

        // Apply gravity
        if (cc.isGrounded && yVelocity < 0)
            yVelocity = -2f;
        else
            yVelocity += gravity * Time.deltaTime;

        moveDir.y = yVelocity;

        // Move with CharacterController
        cc.Move(moveDir * Time.deltaTime);

        // Update animations
        UpdateAnimations(inputDir.magnitude, cc.isGrounded);
    }

    void UpdateAnimations(float inputMagnitude, bool isGrounded)
    {
        if (animator == null)
            return;

        // Only allow Walk animation when grounded and not sitting
        if (isGrounded && !animator.GetBool(sitHash))
        {
            bool isWalking = inputMagnitude > 0.1f;
            animator.SetBool(walkHash, isWalking);
        }
        else
        {
            // Force Walk to false when airborne or sitting
            animator.SetBool(walkHash, false);
        }

        // Reset jump trigger when grounded to allow animation to transition back to Idle/Walk
        if (isGrounded)
            animator.ResetTrigger(jumpHash);
    }

    void SendNetworkUpdate()
    {
        if (net == null) return;
        
        Vector3 pos = transform.position;
        float rotY = characterModel != null ? characterModel.eulerAngles.y : 0f;
        
        bool isRunning = animator.GetBool(runHash);
        bool isWalking = animator.GetBool(walkHash);
        bool isSitting = animator.GetBool(sitHash);
        bool isDancing = animator.GetBool(Animator.StringToHash("Dance"));
        bool isInAir = !cc.isGrounded;
        
        string animState;
        if (isSitting)
            animState = "sit";
        else if (isInAir)
            animState = "jump";
        else if (isRunning)
            animState = "run";
        else if (isWalking)
            animState = "walk";
        else if (isDancing)
            animState = "dance";
        else
            animState = "idle";
        
        net.SendMove(pos, rotY, animState);
    }

    int GetBestEggValue()
    {
        var eggs = FindObjectsByType<EggMilestone>(FindObjectsSortMode.None);

        int best = 0;

        foreach (var egg in eggs)
        {
            if (egg.IsUnlocked() && egg.eggValue > best)
                best = egg.eggValue;
        }

        return best;
    }

    // Call this method from Jump button
    public void OnJumpButtonPressed()
    {
        jumpPressed = true;
    }

    // Call these methods from Sit button (Down and Up)
    public void OnSitButtonPressed()
    {
        sitPressed = true;
    }

    public void OnSitButtonReleased()
    {
        sitPressed = false;
    }
}