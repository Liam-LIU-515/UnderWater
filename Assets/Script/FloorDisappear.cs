using UnityEngine;

public class FloorDisappear : MonoBehaviour
{
    void Start()
    {
        // Subscribe to the Falling event
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnFalling += Disappear;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent errors when the object or scene changes
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnFalling -= Disappear;
        }
    }

    // This method handles the deactivation
    void Disappear()
    {
        Debug.Log(gameObject.name + " has disappeared due to Falling event.");
        
        // Deactivate the current GameObject immediately
        gameObject.SetActive(false);
    }
}