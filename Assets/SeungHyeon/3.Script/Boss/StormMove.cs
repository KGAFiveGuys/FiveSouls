using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormMove : MonoBehaviour
{
    public float initialSpeed = 10.0f; // 초기 속도
    public float acceleration = 1f; // 가속도

    private float currentSpeed;
    public float rotationSpeed = 90.0f; // 회전 속도 (각도/초)

    private void OnEnable()
    {
        currentSpeed = initialSpeed;
    }
    void Update()
    {
        // 회전할 각도를 계산
        float rotationAngle = rotationSpeed * Time.deltaTime;

        // Z 축 방향으로 이동
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
        // 가속도를 더해 속도를 증가
        currentSpeed += acceleration * Time.deltaTime;
        // 오브젝트를 회전
        transform.Rotate(Vector3.up, rotationAngle);
        
    }
}
