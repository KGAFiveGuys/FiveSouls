using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBlinkHighlightOnEnable : MonoBehaviour
{
	[SerializeField] private float frequency = 1f;

	private TextMeshProUGUI text;

	private void Awake()
	{
		TryGetComponent(out text);
	}

	private void OnEnable()
	{
		StartCoroutine(Blink());
	}

	private IEnumerator Blink()
	{
		var offset = Mathf.PI / 2;

		float elapsedTime = 0f;
		while (true)
		{
			elapsedTime += (frequency * 2 * Mathf.PI) * Time.deltaTime;
			var currentAlpha = Mathf.Sin(offset + elapsedTime) / 2 + .5f;

			var origin = text.color;
			text.color = new Color(origin.r, origin.g, origin.b, currentAlpha);

			yield return null;
		}
	}
}
