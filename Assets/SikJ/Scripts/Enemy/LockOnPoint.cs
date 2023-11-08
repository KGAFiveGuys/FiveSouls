using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnPoint : MonoBehaviour
{
    [field:SerializeField] public bool IsLockedOn { get; set; } = false;
    [field:Header("플레이어와 바라보고 있는지 확인.")]
    [field:SerializeField] public bool IsFacingPlayer { get; set; } = false;

    [Header("Linked LockOnPoint. (플레이어와 forward가 일치할 때를 기준)")]
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

    private IEnumerator checkTransition;
    public void StartTransitionCheck()
    {
        checkTransition = CheckTransition();
        StartCoroutine(checkTransition);
    }

    public void StopTransitionCheck()
    {
        StopCoroutine(checkTransition);
    }

    private IEnumerator CheckTransition()
    {
        while (true)
        {
            IsFacingPlayer = CheckFacingWithPlayer();

            float desiredX = _playerController.DesiredRotate.x;
            float desiredY = _playerController.DesiredRotate.y;
            Vector2 desiredDirection = Mathf.Abs(desiredX) < Mathf.Abs(desiredY) ? Vector2.up * desiredY : Vector2.right * desiredX;
            desiredDirection = desiredDirection.normalized;

            // Up/Down
            if (upPoint != null && desiredDirection.y >= .5f)
            {
                if (_playerController.TryChangeLockOnPoint(this, upPoint))
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
                if (_playerController.TryChangeLockOnPoint(this, rightPoint))
                    break;
            }
            else if (leftPoint != null && desiredDirection.x <= -.5f)
            {
                if (_playerController.TryChangeLockOnPoint(this, leftPoint))
                    break;
            }

            yield return null;
        }
    }

    private bool isAlreadyFacing = false;
    private bool CheckFacingWithPlayer()
	{
        var playerForward = _playerController.transform.forward;
        var enemyForward = transform.forward;

        bool isFacing = Vector3.Dot(playerForward, enemyForward) < 0;
        if (!isAlreadyFacing && isFacing)
		{
            isAlreadyFacing = true;
            (leftPoint, rightPoint) = (rightPoint, leftPoint);
        }
		else if (isAlreadyFacing && !isFacing)
		{
            isAlreadyFacing = false;
            (leftPoint, rightPoint) = (rightPoint, leftPoint);
        }

        return isFacing;
    }
}
