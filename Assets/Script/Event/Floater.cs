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
    private float waterHeight = 0f; // 水面高度
    void Start()
    {
        // 如果未手动指定Rigidbody，自动获取
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        if (rigidBody == null)
        {
            Debug.LogError("Need Rigidbody Component！");
        }
        
    }
    
    void FixedUpdate()
    {
        waterHeight = waterSurface.transform.position.y;
        // 检查物体是否在水面以下
        if (transform.position.y < waterHeight)
        {
            // 计算浸没深度比例 (0到1之间)
            float submersionRatio = Mathf.Clamp01((waterHeight - transform.position.y) / depthBeforeSubmerged);

            // 计算浮力大小
            float buoyancyForce = Mathf.Abs(Physics.gravity.y) * displacementAmount * submersionRatio;

            // 施加浮力
            rigidBody.AddForce(new Vector3(0f, buoyancyForce, 0f), ForceMode.Force);

            // 可选：添加阻尼力来模拟水的阻力
            rigidBody.AddForce(-rigidBody.velocity * displacementAmount * 0.1f, ForceMode.Force);
        }
        // if (ball.transform.position.y < waterHeight)
        // {
        //     // 计算浸没深度比例 (0到1之间)
        //     float submersionRatio = Mathf.Clamp01((waterHeight - ball.transform.position.y) / depthBeforeSubmerged);

        //     // 计算浮力大小
        //     float buoyancyForce = Mathf.Abs(Physics.gravity.y) * displacementAmount * submersionRatio;

        //     // 施加浮力
        //     ballRigidBody.AddForce(new Vector3(0f, buoyancyForce, 0f), ForceMode.Force);

        //     // 可选：添加阻尼力来模拟水的阻力
        //     ballRigidBody.AddForce(-rigidBody.velocity * displacementAmount * 0.1f, ForceMode.Force);
        // }
    }
}
