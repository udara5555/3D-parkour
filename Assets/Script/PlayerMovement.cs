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

    void Start()
    {
        cc = GetComponent<CharacterController>();
        switcher = GetComponent<CharacterSwitcher>();
        anim = GetComponentInChildren<Animator>(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // use active skin animator
        if (switcher != null && switcher.ActiveAnimator != null)
            anim = switcher.ActiveAnimator;

        if (anim == null || !anim.gameObject.activeInHierarchy)
            anim = GetComponentInChildren<Animator>(true);

        // mouse yaw (camera direction)
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);

        // input
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // movement relative to mouse direction
        Vector3 input = new Vector3(h, 0f, v);
        Vector3 move = yawRot * input;

        if (move.sqrMagnitude > 1f) move.Normalize();

        cc.Move(move * speed * Time.deltaTime);

        // face movement direction
        Quaternion targetRot;

        if (move.sqrMagnitude > 0.0001f)
            targetRot = Quaternion.LookRotation(move);
        else
            targetRot = yawRot;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 6f   // lower = more delay (try 4–8)
        );


        // gravity
        if (cc.isGrounded) yVelocity = -2f;
        else yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);

        // animations
        anim.SetBool("IsWalking", move.sqrMagnitude > 0.0001f);
        anim.SetBool("Sit", Input.GetKey(KeyCode.C));
    }
}
