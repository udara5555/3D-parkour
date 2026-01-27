using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 6f;
    CharacterController cc;
    Vector3 velocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        cc.Move(move * speed * Time.deltaTime);

        if (cc.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
            if (Input.GetKeyDown(KeyCode.Space))
                velocity.y = jumpForce;
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
