using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Floater : MonoBehaviour
{
    [Header("Floater Setting")]
    public Rigidbody rigidBody;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;
    public GameObject waterSurface;
    // public GameObject ball;
    // public Rigidbody ballRigidBody;
    private float waterHeight = 0f; // water level
    void Start()
    {
        // grab the Rigidbody automatically when it is not assigned
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        if (rigidBody == null)
        {
            Debug.LogError("Need a Rigidbody component.");
        }
        
    }
    
    void FixedUpdate()
    {
        waterHeight = waterSurface.transform.position.y;
        // is the object below the water surface?
        if (transform.position.y < waterHeight)
        {
            // submersion ratio between 0 and 1
            float submersionRatio = Mathf.Clamp01((waterHeight - transform.position.y) / depthBeforeSubmerged);

            // buoyancy force
            float buoyancyForce = Mathf.Abs(Physics.gravity.y) * displacementAmount * submersionRatio;

            // apply the buoyancy
            rigidBody.AddForce(new Vector3(0f, buoyancyForce, 0f), ForceMode.Force);

            // optional damping to mimic water drag
            rigidBody.AddForce(-rigidBody.velocity * displacementAmount * 0.1f, ForceMode.Force);
        }
        // if (ball.transform.position.y < waterHeight)
        // {
        //     // submersion ratio between 0 and 1
        //     float submersionRatio = Mathf.Clamp01((waterHeight - ball.transform.position.y) / depthBeforeSubmerged);

        //     // buoyancy force
        //     float buoyancyForce = Mathf.Abs(Physics.gravity.y) * displacementAmount * submersionRatio;

        //     // apply the buoyancy
        //     ballRigidBody.AddForce(new Vector3(0f, buoyancyForce, 0f), ForceMode.Force);

        //     // optional damping to mimic water drag
        //     ballRigidBody.AddForce(-rigidBody.velocity * displacementAmount * 0.1f, ForceMode.Force);
        // }
    }
}
