using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCutScene : MonoBehaviour
{
    [SerializeField] private Transform Target;
    private Animator animator;

    [SerializeField] private float runSpeed = 20f;

    private void Start()
    {
        TryGetComponent(out animator);
        transform.LookAt(Target);
        StartCutScene();
    }

    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("RUN00_F") || animator.GetCurrentAnimatorStateInfo(0).IsName("Front Twist Flip"))
        {
            Run();
        }
    }

    private void StartCutScene()
    {
        animator.SetTrigger("CutScene");
    }

    private void Run()
    {
        transform.position += transform.TransformDirection(Vector3.forward) * runSpeed * Time.deltaTime;
    }

}
