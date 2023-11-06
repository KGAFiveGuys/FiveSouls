using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] private Vector3 startOffset = Vector3.left * 10f;
    [SerializeField] private Vector3 endOffset = Vector3.right * 10f;
    [SerializeField] private bool isStop = false;
    [SerializeField] private float frequency = 1f;

    private void Start()
    {
        StartCoroutine(Oscillate());
    }
    private IEnumerator Oscillate()
    {
        var startPos = transform.position + startOffset;
        var endPos = transform.position + endOffset;

        float progress = 0f;
        float value = 0f;

        while (true)
        {
            if (!isStop)
                progress += (Mathf.PI * 2) * frequency * Time.deltaTime;

            value = Mathf.Sin(Mathf.PI * 3 / 2 + progress) / 2 + .5f;
            transform.position = Vector3.Lerp(startPos, endPos, value);

            yield return null;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
