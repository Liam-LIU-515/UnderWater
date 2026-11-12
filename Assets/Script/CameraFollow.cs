using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    
    [Range(1f, 100f)]
    public float smoothSpeed = 30f; // Increased for better tracking
    public float rotationSpeed = 100f;

    private float currentAngle = 0f;
    void Start()
    {
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        transform.position = target.position + (rotation * offset);
    }
    void LateUpdate()
    {
        if (target == null) return;

        // 1. Listen for QE input to modify the orbit angle
        if (Input.GetKey(KeyCode.Q))
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentAngle += rotationSpeed * Time.deltaTime;
        }

        // 2. Calculate the rotation based purely on currentAngle
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

        // 3. Define the desired position: Target position + Rotated offset
        Vector3 desiredPosition = target.position + (rotation * offset);

        // 4. Move smoothly toward that desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 5. Ensure camera is always looking at the target
        transform.LookAt(target.position);
    }
}