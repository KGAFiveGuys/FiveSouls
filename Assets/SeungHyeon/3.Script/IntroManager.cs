using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Image BackGround = null;
    [SerializeField] private Image Light = null;
    [SerializeField] private ParticleSystem FireParticle;
    [SerializeField] private Sprite BackGround_Sprite;
    [SerializeField] private TextMeshProUGUI Title = null;
    [SerializeField] private TextMeshProUGUI Start_btn = null;
    [SerializeField] private TextMeshProUGUI Exit_btn = null;
    [SerializeField] private bool Start_check = false;

    private void Start()
    {
        StartCoroutine(FadeTextToFullAlpha(3.5f, BackGround,Light, Title, Start_btn, Exit_btn));
    }
    public IEnumerator FadeTextToFullAlpha(float t, Image i,Image light, TextMeshProUGUI title, TextMeshProUGUI start_btn, TextMeshProUGUI exit_btn)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        light.color = new Color(light.color.r, light.color.g, light.color.b, 0);
        title.color = new Color(title.color.r, title.color.g, title.color.b, 0);
        start_btn.color = new Color(start_btn.color.r, start_btn.color.g, start_btn.color.b, 0);
        exit_btn.color = new Color(exit_btn.color.r, exit_btn.color.g, exit_btn.color.b, 0);

        while (i.color.b > 0.0f)
        {
            i.color = new Color(i.color.r - (Time.deltaTime / t), i.color.g - (Time.deltaTime / t), i.color.b - (Time.deltaTime / t), i.color.a);
            yield return null;
        }

        while (title.color.a < 1.0f)
        {
            title.color = new Color(title.color.r, title.color.g, title.color.b, title.color.a + (Time.deltaTime / t));
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        //while (title.rectTransform.position.y <= LastTransform.position.y)
        //{
        //    title.rectTransform.position = new Vector3(title.rectTransform.position.x, title.rectTransform.position.y + 0.1f);
        //    yield return null;
        //}

        //while (start_btn.color.a < 1.0f)
        //{
        //    start_btn.color = new Color(start_btn.color.r, start_btn.color.g, start_btn.color.b, start_btn.color.a + (Time.deltaTime / t));
        //    yield return null;
        //}

        //while (exit_btn.color.a < 1.0f)
        //{
        //    exit_btn.color = new Color(exit_btn.color.r, exit_btn.color.g, exit_btn.color.b, exit_btn.color.a + (Time.deltaTime / t));
        //    yield return null;
        //}
        while(light.color.a < 1.0f)
        {
            light.color = new Color(light.color.r, light.color.g, light.color.b, light.color.a + (Time.deltaTime / t));
            yield return null;
        }
        start_btn.color = new Color(1, 1, 1, 1);
        exit_btn.color = new Color(1, 1, 1, 1);
        //스프라이트 바꾸고 파티클 실행
        i.color = new Color(1, 1, 1, 1);
        i.sprite = BackGround_Sprite;
        FireParticle.Play();
        while (light.color.a > 0.0f)
        {
            light.color = new Color(light.color.r, light.color.g, light.color.b, light.color.a - (Time.deltaTime / t) * 2f);
            yield return null;
        }
        light.gameObject.SetActive(false);
    }
}
