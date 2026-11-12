using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public float speed = 10f;
    // Reference to the main camera transform
    public Transform cameraTransform;

    void Update()
    {
        // 1. Get basic WASD input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 2. Get the camera's forward and right vectors
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // 3. Flatten the vectors (set Y to 0) to prevent moving into the ground
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 4. Calculate the relative movement direction
        Vector3 moveDirection = (camForward * vertical) + (camRight * horizontal);

        // 5. Apply the movement in world space
        transform.position += moveDirection * speed * Time.deltaTime;
    }
}