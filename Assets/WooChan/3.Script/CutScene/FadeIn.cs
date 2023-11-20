using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private Image FadeImage;
    private Color TargetColor = new Color(0, 0, 0, 1);
    [SerializeField] private float FadeSpeed = 1f;

    private void Start()
    {
        StartCoroutine(StartFadeIn());
    }

    private IEnumerator StartFadeIn()
    {
        while (FadeImage.color.a > 0)
        {
            FadeImage.color = TargetColor;
            TargetColor.a -= Time.deltaTime * FadeSpeed;
            yield return null;
        }
        yield break;
    }

    private IEnumerator StartFadeOut()
    {
        while (FadeImage.color.a < 1)
        {
            FadeImage.color = TargetColor;
            TargetColor.a += Time.deltaTime * FadeSpeed;
            yield return null;
        }
        SceneManager.LoadScene("WooChan");
        yield break;
    }
    public void FadeOut()
    {
        StartCoroutine(StartFadeOut());
    }
}
