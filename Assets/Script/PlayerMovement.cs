using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.8f;

    public float mouseSensitivity = 2f;
    float yaw;
    float yVelocity;

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

        // mouse rotate
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // move forward/back
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 move = transform.forward * v;
        cc.Move(move * speed * Time.deltaTime);

        // gravity
        if (cc.isGrounded) yVelocity = -2f;
        else yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);

        // animations
        anim.SetBool("IsWalking", Mathf.Abs(v) > 0.1f);

        if (Input.GetKeyDown(KeyCode.C))
            anim.SetBool("Sit", true);

        if (Input.GetKeyUp(KeyCode.C))
            anim.SetBool("Sit", false);
    }

    public void RefreshAnimator()
    {
        anim = GetComponentInChildren<Animator>(true);
    }
}
