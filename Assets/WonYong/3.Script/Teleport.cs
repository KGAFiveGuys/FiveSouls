using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Vector3 positionToTeleport = new Vector3(400.95694f, -976.799988f, 551.172363f);
    private PlayerController playerController;
    [SerializeField] private float count= 3f;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(TeleportPlayer_co());
        }
    }

    private IEnumerator TeleportPlayer_co()
    {
        playerController.gameObject.GetComponent<Rigidbody>().useGravity = false;
        playerController.gameObject.GetComponent<ConstantForce>().enabled = false;
        playerController.gameObject.transform.position = positionToTeleport;
        playerController.gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);

        yield return new WaitForSeconds(count);

        playerController.gameObject.GetComponent<Rigidbody>().useGravity = true;
        playerController.gameObject.GetComponent<ConstantForce>().enabled = true;
    }
}
