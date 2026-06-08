using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DiverMovement : MonoBehaviour
{
    public float moveSpeed = 4f;

    public float verticalSpeed = 2f;

    public float smoothTime = 0.15f;

    [Header("Upper bound")]
    [Tooltip("Optional water surface. The diver cannot rise above this plane. Leave empty to disable.")]
    public Transform waterSurface;

    [Tooltip("Offset from the water surface for the clamp. Negative keeps the camera below the surface.")]
    public float surfaceOffset = -0.7f;

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

        // Clamp at the water surface so the diver cannot go above the water.
        if (waterSurface != null)
        {
            float maxY = waterSurface.position.y + surfaceOffset;
            if (transform.position.y > maxY)
            {
                Vector3 clamped = transform.position;
                clamped.y = maxY;
                transform.position = clamped;
            }
        }
    }
}