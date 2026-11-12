using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    // Use OnTriggerEnter to detect when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // 2. Broadcast the Falling event via the EventManager
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerFalling();
                Debug.Log("FallTrigger: Player detected! Broadcasting Falling event.");
            }
            else
            {
                Debug.LogWarning("FallTrigger: EventManager Instance not found!");
            }
        }
    }
}