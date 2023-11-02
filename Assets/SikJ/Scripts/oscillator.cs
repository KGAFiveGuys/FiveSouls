using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oscillator : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private bool isStop = false;
    [SerializeField] private float frequency = 1f;

    private void Start()
    {
        StartCoroutine(Oscillate());
    }

    private IEnumerator Oscillate()
    {
        float progress = 0f;
        float value = 0f;
        while (true)
        {
            if (!isStop)
                progress += (Mathf.PI * 2) * frequency * Time.deltaTime;

            var startOffset = Mathf.PI * 3 / 2;
            value = Mathf.Sin(startOffset + progress) / 2 + .5f;
            transform.position = Vector3.Lerp(start.position, end.position, value);

            yield return null;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
