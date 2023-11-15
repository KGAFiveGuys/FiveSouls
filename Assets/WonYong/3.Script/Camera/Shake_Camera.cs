using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Shake_Camera : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera camera_;

    Vector3 cameraPos;

    [SerializeField] [Range(0.01f, 2f)] float ShakeRangeX = 0.05f;
    [SerializeField] [Range(0.01f, 2f)] float ShakeRangeY = 0.05f;
    [SerializeField] [Range(0.1f, 2f)] float duration = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            Shake();
        }
    }

    private void Shake()
    {
        cameraPos = camera_.transform.position;
        InvokeRepeating("StartShake", 0, 0.005f);
        Invoke("StopShake",duration);
    }

    private void StartShake()
    {
        float cameraPosx = Random.value * ShakeRangeX * 2 - ShakeRangeX;
        float cameraPosY = Random.value * ShakeRangeY * 2 - ShakeRangeY;
        Vector3 cameraPos = camera_.transform.position;
        cameraPos.x += cameraPosx;
        cameraPos.y += cameraPosY;
        camera_.transform.position = cameraPos;
    }
    private void StopShake()
    {
        CancelInvoke("StartShake");
        camera_.transform.position = cameraPos;
    }
}
