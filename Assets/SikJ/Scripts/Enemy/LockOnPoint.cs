using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnPoint : MonoBehaviour
{
    [field:SerializeField] public bool IsLockedOn { get; set; } = false;

    [Header("Linked LockOnPoint")]
    [SerializeField] private LockOnPoint leftPoint;
    [SerializeField] private LockOnPoint rightPoint;
    [SerializeField] private LockOnPoint upPoint;
    [SerializeField] private LockOnPoint downPoint;

    public Collider LockOnCollider { get; private set; }
    public GameObject EnemyObject { get; private set; }
    private PlayerController _playerController;

    private void Awake()
    {
        LockOnCollider = GetComponent<Collider>();
        EnemyObject = transform.parent.gameObject;
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private IEnumerator transitionCheck;
    public void StartTransitionCheck()
    {
        transitionCheck = CheckTransition();
        StartCoroutine(transitionCheck);
    }

    public void StopTransitionCheck()
    {
        StopCoroutine(transitionCheck);
    }

    private IEnumerator CheckTransition()
    {
        while (true)
        {
            var desiredX = _playerController.DesiredRotate.x;
            var desiredY = _playerController.DesiredRotate.y;
            Vector2 desiredDirection = Mathf.Abs(desiredX) < Mathf.Abs(desiredY) ? Vector2.up * desiredY : Vector2.right * desiredX;
            desiredDirection = desiredDirection.normalized;

            // Up/Down
            if (upPoint != null && desiredDirection.y >= .5f)
            {
                if(_playerController.TryChangeLockOnPoint(this, upPoint))
                    break;
            }
            else if (downPoint != null && desiredDirection.y <= -.5f)
            {
                if(_playerController.TryChangeLockOnPoint(this, downPoint))
                    break;
            }

            // Right/Left
            if (rightPoint != null && desiredDirection.x >= .5f)
            {
                if(_playerController.TryChangeLockOnPoint(this, rightPoint))
                    break;
            }
            else if (leftPoint != null && desiredDirection.x <= -.5f)
            {
                if(_playerController.TryChangeLockOnPoint(this, leftPoint))
                    break;
            }

            yield return null;
        }
    }
}
