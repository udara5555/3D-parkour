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
    private int jumpHash = Animator.StringToHash("Jump");
    private int sitHash = Animator.StringToHash("Sit");

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
        
        bool isWalking = animator.GetBool(walkHash);
        bool isSitting = animator.GetBool(sitHash);
        bool isInAir = !cc.isGrounded;
        
        string animState = isSitting ? "sit" : (isInAir ? "jump" : (isWalking ? "walk" : "idle"));
        
        Debug.Log($"Sending: pos={pos}, rot={rotY}, anim={animState}");  // Add this for debugging
        net.SendMove(pos, rotY, animState);
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