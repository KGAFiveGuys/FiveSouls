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
        transitionThreshold = 0f;
        transitionCheck = CheckTransition();
        StartCoroutine(transitionCheck);
    }

    public void StopTransitionCheck()
    {
        StopCoroutine(transitionCheck);
    }

    [SerializeField] private float transitionThreshold = 1f;
    private IEnumerator CheckTransition()
    {
        // Delay after change
        float elapsedTimeAfterChanged = transitionThreshold;
        while (elapsedTimeAfterChanged > 0)
        {
            elapsedTimeAfterChanged -= Time.deltaTime;
            yield return null;
        }

        while (true)
        {
            var desiredX = _playerController.DesiredRotate.x;
            var desiredY = _playerController.DesiredRotate.y;
            Vector2 desiredDirection = Mathf.Abs(desiredX) < Mathf.Abs(desiredY) ? Vector2.up * desiredY : Vector2.right * desiredX;
            desiredDirection = desiredDirection.normalized;

            // Up/Down
            if (upPoint != null && desiredDirection.y >= .5f)
            {
                _playerController.ChangeLockOnPoint(this, upPoint);
                break;
            }
            else if (downPoint != null && desiredDirection.y <= -.5f)
            {
                _playerController.ChangeLockOnPoint(this, downPoint);
                break;
            }

            // Right/Left
            if (rightPoint != null && desiredDirection.x >= .5f)
            {
                _playerController.ChangeLockOnPoint(this, rightPoint);
                break;
            }
            else if (leftPoint != null && desiredDirection.x <= -.5f)
            {
                _playerController.ChangeLockOnPoint(this, leftPoint);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
