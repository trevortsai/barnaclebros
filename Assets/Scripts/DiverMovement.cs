using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DiverMovement : MonoBehaviour
{
    public float moveSpeed = 4f;

    public float verticalSpeed = 2f;

    public float smoothTime = 0.15f;

    private CharacterController controller;

    private Vector3 currentVelocity;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float y = 0f;

        // smoother vertical movement
        if (Input.GetKey(KeyCode.Space))
        {
            y = verticalSpeed;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            y = -verticalSpeed;
        }

        Vector3 targetDirection =
            transform.right * x +
            transform.forward * z +
            transform.up * y;

        targetDirection *= moveSpeed;

        // smooth underwater movement
        moveDirection = Vector3.SmoothDamp(
            moveDirection,
            targetDirection,
            ref currentVelocity,
            smoothTime
        );

        controller.Move(moveDirection * Time.deltaTime);
    }
}