using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CinematicCameraManager : MonoBehaviour
{
    [SerializeField] private InputAction cancel;

    [SerializeField] private GameObject camera;
    [SerializeField] private CinemachineSmoothPath track;
    [SerializeField] private CinemachineDollyCart cart;

    [SerializeField] private AnimationCurve SpeedOverPath;

    private PlayerController playerController;
    private PlayerHUDController playerHUD;
    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerHUD = FindObjectOfType<PlayerHUDController>();
    }

    private void OnEnable()
    {
        cancel.performed += CancelCinematic;
        cancel.Enable();
    }

    private void OnDisable()
    {
        cancel.performed -= CancelCinematic;
        cancel.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")
            && cart.m_Position != track.PathLength)
        {
            playerController.MoveDirection = Vector3.zero;
            playerController.ControlState = ControlState.Uncontrollable;
            StartCoroutine(StartCinematic());
        }
    }

    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 30f;
    private bool isCanceled = false;
    private IEnumerator StartCinematic()
    {
        playerHUD.FadeOutPlayerHUD();
        camera.SetActive(true);

        var trackEndPosition = track.transform.position + track.m_Waypoints[track.m_Waypoints.Length - 1].position;
        while (camera.transform.position != trackEndPosition
               && !isCanceled)
        {
            float travelRate = cart.m_Position / track.PathLength;
            cart.m_Speed = minSpeed + (maxSpeed - minSpeed) * SpeedOverPath.Evaluate(travelRate);
            yield return null;
        }

        cart.m_Position = track.PathLength;
        camera.SetActive(false);
        playerHUD.FadeInPlayerHUD();
        playerController.ControlState = ControlState.Controllable;
    }

    private void CancelCinematic(InputAction.CallbackContext context)
    {
        isCanceled = true;
    }
}
