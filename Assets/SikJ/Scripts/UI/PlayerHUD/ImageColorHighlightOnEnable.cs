using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageColorHighlightOnEnable : MonoBehaviour
{
	[Header("Color")]
	[SerializeField] private Color startColor;
	[SerializeField] private float colorHighlightDuration = 1f;

	private Image image;
	private Color originColor;
	private void Awake()
	{
		TryGetComponent(out image);
		originColor = image.color;
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
			image.color = Color.Lerp(startColor, originColor, elapsedTime/colorHighlightDuration);
			yield return null;
		}
		image.color = originColor;
	}
}
