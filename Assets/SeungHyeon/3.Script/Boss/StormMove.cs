using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormMove : MonoBehaviour
{
    public float initialSpeed = 10.0f; // �ʱ� �ӵ�
    public float acceleration = 1f; // ���ӵ�

    private float currentSpeed;
    public float rotationSpeed = 90.0f; // ȸ�� �ӵ� (����/��)

    private void OnEnable()
    {
        currentSpeed = initialSpeed;
    }
    void Update()
    {
        // ȸ���� ������ ���
        float rotationAngle = rotationSpeed * Time.deltaTime;

        // Z �� �������� �̵�
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
        // ���ӵ��� ���� �ӵ��� ����
        currentSpeed += acceleration * Time.deltaTime;
        // ������Ʈ�� ȸ��
        transform.Rotate(Vector3.up, rotationAngle);
        
    }
}
