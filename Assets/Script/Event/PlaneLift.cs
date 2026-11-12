using UnityEngine;

public class PlaneLift : MonoBehaviour
{
    [Header("Rising Set")]
    public float ceil = 8f;
    public float riseSpeed = 2f;
    public KeyCode riseKey = KeyCode.Space;

    // Track if the auto-rise mode is active
    private bool isAutoRising = false;

    void Start()
    {
        // Subscribe to the Falling event from EventManager
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnFalling += StartAutoRise;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnFalling -= StartAutoRise;
        }
    }

    void Update()
    {
        // 1. Manual Rise with Space Key
        if (Input.GetKey(riseKey))
        { 
            Rise();
            // Optional: Manually pressing key could disable auto-mode
            isAutoRising = false; 
        } 

        // 2. Automatic Rise after receiving Falling event
        if (isAutoRising)
        {
            Rise();
        }
    }

    // Logic to handle rising within the limit
    void Rise()
    {
        if (transform.position.y < ceil)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
        }
        else
        {
            // Stop auto-rising once it reaches the ceiling
            isAutoRising = false;
        }
    }

    // This method will be called when the event is broadcasted
    void StartAutoRise()
    {
        isAutoRising = true;
        Debug.Log("PlaneLift: Event received, starting auto-rise.");
    }
}