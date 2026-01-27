using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    float yVelocity;
    public float gravity = -9.8f;

    


    CharacterController cc;
    Animator anim;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // Rotate left / right
        transform.Rotate(Vector3.up * h * 180f * Time.deltaTime);

        // Move forward / backward
        Vector3 move = transform.forward * v;
        cc.Move(move * speed * Time.deltaTime);


        if (cc.isGrounded)
            yVelocity = -2f;
        else
            yVelocity += gravity * Time.deltaTime;

        cc.Move(Vector3.up * yVelocity * Time.deltaTime);

        anim.SetBool("IsWalking", move.magnitude > 0.1f);

        if (Input.GetKeyDown(KeyCode.C))
            anim.SetBool("Sit", true);

        if (Input.GetKeyUp(KeyCode.C))
            anim.SetBool("Sit", false);
    }

}
