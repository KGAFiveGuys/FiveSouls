using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private Transform elevatorPlatform; // 엘리베이터 플랫폼의 Transform 컴포넌트
    [SerializeField] private float elevatorSpeed = 5.0f; // 엘리베이터의 하강 속도
    [SerializeField] private float maxDistance = 30f; // 최대 이동거리
    [SerializeField] private float Rotation_Speed;

    private Elevator_btn elevatorButton;
    // 이동한 거리
    private float movedDistance = 0f;

    private void Awake()
    {
        elevatorButton = FindObjectOfType<Elevator_btn>();
    }

    private void Update()
    {
        if (Elevator_btn.eleva_btn)
        {
            Elevator_btn.eleva_btn = !Elevator_btn.eleva_btn;
            StartCoroutine(MoveElevator());
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }

        if (Elevator_btn.eleva_btn)
        {
            Elevator_btn.eleva_btn = false;
            StartCoroutine(MoveElevator());
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform != null)
        {
            collision.transform.SetParent(null);
            movedDistance = 0;
        }

    }

    private IEnumerator MoveElevator()
    {
        while (movedDistance < maxDistance)
        {
            float distanceToMove = elevatorSpeed * Time.deltaTime;
            elevatorPlatform.Translate(Vector3.down * distanceToMove);
            movedDistance += distanceToMove; // 이동한 거리
            float rotationAmount = elevatorSpeed * Time.deltaTime * Rotation_Speed;
            elevatorPlatform.rotation *= Quaternion.Euler(0f, rotationAmount, 0f);
            yield return null;
        }
        elevatorButton.objectMaterial.color = Color.red;

    }
}
