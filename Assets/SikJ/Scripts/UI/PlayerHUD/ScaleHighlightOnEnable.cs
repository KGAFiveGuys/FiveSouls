using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleHighlightOnEnable : MonoBehaviour
{
	[Header("Scale")]
	[SerializeField] private float startScale;
	[SerializeField] private float scaleHighlightDuration = 1f;

	private Vector3 originScale;
	private RectTransform rectTransform;
	private void Awake()
	{
		TryGetComponent(out rectTransform);
		originScale = transform.localScale;
	}

	private void OnEnable()
	{
		StartCoroutine(HighlightScale());
	}

	private IEnumerator HighlightScale()
	{
		float elapsedTime = 0f;
		while (elapsedTime < scaleHighlightDuration)
		{
			elapsedTime += Time.deltaTime;
			transform.localScale = Vector3.Lerp(originScale * startScale, originScale, elapsedTime/scaleHighlightDuration);
			yield return null;
		}
		transform.localScale = originScale;
	}
}
