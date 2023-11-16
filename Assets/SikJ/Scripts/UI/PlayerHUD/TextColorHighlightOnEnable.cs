using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextColorHighlightOnEnable : MonoBehaviour
{
	[Header("Color")]
	[SerializeField] private Color startColor;
	[SerializeField] private float colorHighlightDuration = 1f;

	private TextMeshProUGUI text;
	private Color originColor;
	private void Awake()
	{
		TryGetComponent(out text);
		originColor = text.color;
	}

	private void OnEnable()
	{
		StartCoroutine(HighlightColor());
	}

	private IEnumerator HighlightColor()
	{
		float elapsedTime = 0f;
		while (elapsedTime < colorHighlightDuration)
		{
			elapsedTime += Time.deltaTime;
			text.color = Color.Lerp(startColor, originColor, elapsedTime / colorHighlightDuration);
			yield return null;
		}
		text.color = originColor;
	}
}
