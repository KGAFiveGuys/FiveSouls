using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInteractionUI : MonoBehaviour
{
    [SerializeField] private InputAction mouseInput;
    [SerializeField] private Vector2 moveLimit = Vector2.one * 40f;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        mouseInput.performed += LerpPosition;
        mouseInput.Enable();
    }

    private void OnDisable()
    {
        mouseInput.performed -= LerpPosition;
        mouseInput.Disable();
    }

    private Vector2 mouseDelta = Vector2.zero;
    private void LerpPosition(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();

        if (lastLerpPosition != null)
        {
            StopCoroutine(lastLerpPosition);
            lastLerpPosition = null;
        }

        lastLerpPosition = StartLerpPosition();
        StartCoroutine(lastLerpPosition);
    }

    private IEnumerator lastLerpPosition = null;
    [SerializeField] private float lerpDistance = 30f;
    [SerializeField] private float lerpDuration = 1f;
    [SerializeField] private AnimationCurve lerpSpeedOverTime;
    private IEnumerator StartLerpPosition()
    {
        var startPos = rectTransform.anchoredPosition;
        var endPos = startPos - mouseDelta * lerpDistance;
        
        // Limit end position
        if (Mathf.Abs(endPos.x) > moveLimit.x)
            endPos.x = endPos.x > 0 ? moveLimit.x : -moveLimit.x;
        else if (Mathf.Abs(endPos.y) > moveLimit.y)
            endPos.y = endPos.y > 0 ? moveLimit.y : -moveLimit.y;

        float elapsedTime = 0f;
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, lerpSpeedOverTime.Evaluate(elapsedTime / lerpDuration));
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;

        lastLerpPosition = null;
    }
}
