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

            // make sure characterModel points to the root of the active skin
            if (switcher.CurrentSkin == 0)  // default player
                characterModel = switcher.player.transform.GetChild(0); // root
            else
                characterModel = switcher.others[switcher.CurrentSkin - 1].transform.GetChild(0); // root
        }
            

        if (anim == null || !anim.gameObject.activeInHierarchy)
            anim = GetComponentInChildren<Animator>(true);

        // mouse yaw (camera direction)
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);

        // input
        float h = Input.GetAxisRaw("Horizontal") + joystick.Horizontal; // A/D
        float v = Input.GetAxisRaw("Vertical") + joystick.Vertical;   // W/S

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

        //transform.rotation = Quaternion.Euler(0f, characterModel.eulerAngles.y, 0f);

        if (move.sqrMagnitude > 1f) move.Normalize();

        cc.Move(move * speed * Time.deltaTime);

        // rotate animation only (not player)
        Transform model = anim.transform.root;

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
        {
            isJumping = false;
        }

        // animations
        bool isWalking = move.sqrMagnitude > 0.0001f && cc.isGrounded;
        bool isSitting = Input.GetKey(KeyCode.C) || sitPressed;
        bool isInAir = !cc.isGrounded; // treat falling as "in air" for network state

        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("Sit", isSitting);
        anim.SetBool("Jump", isInAir);


        if (net != null && net.IsInRoom)   // if you don’t have IsInRoom, use: net.room != null
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= 0.05f)
            {
                sendTimer = 0f;

                // Combine logic for animation state into a single variable
                string action =
                    isSitting ? "sit" :
                    isInAir ? "jump" :
                    isWalking ? "walk" : "idle";

                net.SendMove(transform.position, characterModel.rotation.eulerAngles.y, action);
            }
        }

    }

    public void OnJump()
    {
        jumpPressed = true;
    }

    public void OnSitDown()
    {
        sitPressed = true;
    }

    public void OnSitUp()
    {
        sitPressed = false;
    }
}
