using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Singleton instance for global access
    public static EventManager Instance;

    // Define the Falling event using an Action
    public event Action OnFalling;

    void Awake()
    {
        // Ensure there is only one instance of EventManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to trigger the Falling event
    public void TriggerFalling()
    {
        // Check if anyone is listening before invoking
        if (OnFalling != null)
        {
            OnFalling.Invoke();
            Debug.Log("Event: Falling has been broadcasted.");
        }
    }
}