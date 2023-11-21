using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinematicCameraManager : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private CinemachineSmoothPath track;
    [SerializeField] private CinemachineDollyCart cart;

    [SerializeField] private AnimationCurve SpeedOverPath;

    private PlayerController playerController;
    private PlayerHUDController playerHUD;
    private CinemachineVirtualCamera virtualCamera;
    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerHUD = FindObjectOfType<PlayerHUDController>();
        virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController.MoveDirection = Vector3.zero;
            playerController.ControlState = ControlState.Uncontrollable;
            StartCoroutine(StartCinematic());
        }
    }

    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 30f;
    private IEnumerator StartCinematic()
    {
        playerHUD.FadeOutPlayerHUD();
        camera.SetActive(true);

        var trackEndPosition = track.transform.position + track.m_Waypoints[track.m_Waypoints.Length - 1].position;
        while (camera.transform.position != trackEndPosition)
        {
            float travelRate = cart.m_Position / track.PathLength;
            cart.m_Speed = minSpeed + (maxSpeed - minSpeed) * SpeedOverPath.Evaluate(travelRate);

            yield return null;
        }

        camera.SetActive(false);
        playerHUD.FadeInPlayerHUD();
        playerController.ControlState = ControlState.Controllable;
    }
}
