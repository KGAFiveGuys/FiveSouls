using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCutScene : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject LeftEye;
    [SerializeField] private GameObject RightEye;
    [SerializeField] private GameObject EyeLight;
    private Animator animator;

    [SerializeField] private float fallingSpeed = 100f;

    private void Start()
    {
        TryGetComponent(out animator);
        //transform.LookAt(Target);
        StartCutScene();
    }

    private void Update()
    {
        if (transform.position.y < 20 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Kneeling Down"))
        {
            animator.SetTrigger("Fall");
        }
        if (transform.position.y > 0)
        {
            Falling();
        }
        else if(transform.position.y <= 0)
        {
            transform.position = new Vector3(0, 0, 50);
            
        }
    }

    private void StartCutScene()
    {
        animator.SetTrigger("FallingStart");
    }

    private void Falling()
    {
        transform.position += transform.TransformDirection(Vector3.down) * fallingSpeed * Time.deltaTime;
    }

}
