using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cremoa : MonoBehaviour
{
    public bool isOver = false;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOver = true;

            if (isOver)
            {
                playerController.MoveDirection = Vector3.zero;
                playerController.ControlState = ControlState.Uncontrollable;
            }
        }
    }
}
