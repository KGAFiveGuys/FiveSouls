using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cremoa : MonoBehaviour
{
    public bool isOver = false;
    private PlayerController playerController;
    private AudioSource audioSource;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        audioSource = transform.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isOver)
            {
                isOver = true;
                audioSource.Stop();
                playerController.MoveDirection = Vector3.zero;
                playerController.ControlState = ControlState.Uncontrollable;
            }
        }
    }
}
