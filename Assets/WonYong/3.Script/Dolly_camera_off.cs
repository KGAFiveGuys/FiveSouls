using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Dolly_camera_off : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private GameObject dollyCart;
    [SerializeField] private GameObject Dolly_camera;
    [SerializeField] private Collider col;
    Vector3 targetPosition = new Vector3(401.7f, -702.9f, 961.7f);
    float positionThreshold = 1f;

    private void OnEnable()
    {
        player = GameObject.Find("Player");
        col.isTrigger = false;
    }

    private void Start()
    {
        player.SetActive(false);
    }

    private void Update()
    {
              
        float distance = Vector3.Distance(dollyCart.transform.position, targetPosition);
        //Mathf.Epsilon 매ㅐㅐㅐㅐ우작은수
        if (distance < positionThreshold)
        {
            player.SetActive(true);
            Dolly_camera.SetActive(false);
        }
    }



}