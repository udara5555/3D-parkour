using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.8f;
    public float mouseSensitivity = 2f;

    float yVelocity;
    float yaw;

    CharacterController cc;
    Animator anim;
    CharacterSwitcher switcher;

    ColyseusManager net;
    float sendTimer;

    public Transform characterModel;

    public float jumpForce = 5f;
    bool isJumping = false;

    public FixedJoystick joystick;
    public bool jumpPressed;
    public bool sitPressed;

    public int localClickCount { get; private set; } = 0;

    // Auto-movement state
    private bool isAutoMoving = false;
    private float lastRecordedSpeed = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        switcher = GetComponent<CharacterSwitcher>();
        anim = GetComponentInChildren<Animator>(true);
        net = FindAnyObjectByType<ColyseusManager>();
    }

    void Update()
    {
        // use active skin animator
        if (switcher != null && switcher.ActiveAnimator != null)
        {
            anim = switcher.ActiveAnimator;

            if (switcher.CurrentSkin == 0)
                characterModel = switcher.player.transform.GetChild(0);
            else
                characterModel = switcher.others[switcher.CurrentSkin - 1].transform.GetChild(0);
        }

        if (anim == null || !anim.gameObject.activeInHierarchy)
            anim = GetComponentInChildren<Animator>(true);

        // FREEZE during countdown — but allow clicking
        bool isFrozen = (net != null && net.CurrentPhase == "countdown");

        if (isFrozen)
        {
            // NEW: detect click/tap and send to server during countdown
            if (Input.GetMouseButtonDown(0))
            {
                if (localClickCount == 0)
                {
                    int bestEgg = GetBestEggValue();
                    int bonus = bestEgg / 10;

                    localClickCount = bonus;

                    net?.SendInitialBonus(bonus);
                }

                localClickCount++;
                net?.SendClick();
                Debug.Log("Local Clicks: " + localClickCount + " | Server Clicks: " + net.ServerClickCount);
            }

            // stop animations
            anim.SetBool("IsWalking", false);
            anim.SetBool("Sit", false);
            anim.SetBool("Jump", false);

            // gravity still applies
            if (cc.isGrounded)
                yVelocity = -2f;
            else
                yVelocity += gravity * Time.deltaTime;

            cc.Move(Vector3.up * yVelocity * Time.deltaTime);
            return; // skip movement input
        }

        // reset click count when race starts
        if (net != null && net.CurrentPhase == "racing" && localClickCount > 0)
            localClickCount = 0;

        // Check for speed change during racing phase
        if (net != null && net.CurrentPhase == "racing")
        {
            if (net.LocalPlayerSpeed != lastRecordedSpeed)
            {
                isAutoMoving = true;
                lastRecordedSpeed = net.LocalPlayerSpeed;
                Debug.Log($"Speed changed to {lastRecordedSpeed}, enabling auto-movement in +Z direction");
            }
        }

        // Handle automatic movement in +Z direction
        if (isAutoMoving && net != null && net.CurrentPhase == "racing")
        {
            Vector3 autoMove = Vector3.forward * net.LocalPlayerSpeed * Time.deltaTime;
            cc.Move(autoMove);

            // Apply gravity
            if (cc.isGrounded && !isJumping)
                yVelocity = -2f;
            else
                yVelocity += gravity * Time.deltaTime;

            cc.Move(Vector3.up * yVelocity * Time.deltaTime);

            // Play walking animation
            anim.SetBool("IsWalking", true);
            anim.SetBool("Sit", false);
            anim.SetBool("Jump", false);

            // Keep character facing +Z direction (body and model)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            if (characterModel != null)
                characterModel.localRotation = Quaternion.LookRotation(Vector3.forward);

            // Send network update
            if (net.IsInRoom)
            {
                sendTimer += Time.deltaTime;
                if (sendTimer >= 0.05f)
                {
                    sendTimer = 0f;
                    net.SendMove(transform.position, 0f, "walk");
                }
            }

            return; // Skip normal movement controls
        }

        // mouse yaw (camera direction)
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);

        // input
        float h = Input.GetAxisRaw("Horizontal") + joystick.Horizontal;
        float v = Input.GetAxisRaw("Vertical") + joystick.Vertical;

        // Jump input
        if (cc.isGrounded && (Input.GetKeyDown(KeyCode.Space) || jumpPressed))
        {
            yVelocity = jumpForce;
            isJumping = true;
        }
        jumpPressed = false;

        // movement relative to mouse direction
        Vector3 input = new Vector3(h, 0f, v);
        Vector3 move = yawRot * input;

        if (move.sqrMagnitude > 1f) move.Normalize();

        // use server-assigned speed during racing phase
        float currentSpeed = speed;
        if (net != null && net.CurrentPhase == "racing")
            currentSpeed = net.LocalPlayerSpeed;

        cc.Move(move * currentSpeed * Time.deltaTime);

        // rotate animation only
        Vector3 localInput = new Vector3(h, 0f, v);
        if (localInput.sqrMagnitude > 0.01f)
        {
            Quaternion animRot = Quaternion.LookRotation(localInput);
            characterModel.localRotation = Quaternion.Slerp(
                characterModel.localRotation,
                animRot,
                Time.deltaTime * 10f
            );
        }

        // face movement direction
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // gravity
        if (cc.isGrounded && !isJumping)
            yVelocity = -2f;
        else
            yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);

        if (cc.isGrounded)
            isJumping = false;

        // animations
        bool isWalking = move.sqrMagnitude > 0.0001f && cc.isGrounded;
        bool isSitting = Input.GetKey(KeyCode.C) || sitPressed;
        bool isInAir = !cc.isGrounded;

        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("Sit", isSitting);
        anim.SetBool("Jump", isInAir);

        if (net != null && net.IsInRoom)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= 0.05f)
            {
                sendTimer = 0f;
                string action =
                    isSitting ? "sit" :
                    isInAir ? "jump" :
                    isWalking ? "walk" : "idle";
                net.SendMove(transform.position, characterModel.rotation.eulerAngles.y, action);
            }
        }
    }

    public void OnJump() => jumpPressed = true;
    public void OnSitDown() => sitPressed = true;
    public void OnSitUp() => sitPressed = false;

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
}