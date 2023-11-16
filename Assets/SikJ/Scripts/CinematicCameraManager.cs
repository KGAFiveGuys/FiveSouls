using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinematicCameraManager : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private CinemachineSmoothPath track;
    [SerializeField] private CinemachineDollyCart cart;

    private PlayerController playerController;
    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Start()
    {
        playerController.ControlState = ControlState.Uncontrollable;
        StartCoroutine(StartCinematic(1f));
    }

    private IEnumerator StartCinematic(float delay)
    {
        yield return new WaitForSeconds(delay);

        camera.SetActive(true);
        cart.m_Speed = 25f;

        // Wait until finish
        while (camera.transform.position != track.transform.position + track.m_Waypoints[track.m_Waypoints.Length - 1].position)
        {
            yield return null;
        }

        camera.SetActive(false);
        playerController.ControlState = ControlState.Controllable;
    }
}
