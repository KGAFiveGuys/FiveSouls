using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour , IPointerClickHandler , IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ParticleSystem StartEffect;
    [SerializeField] ParticleSystem ExitEffect;
    public void OnPointerClick(PointerEventData eventdata)
    {
        if(eventdata.button == PointerEventData.InputButton.Left)
        {
            if(name.Equals("StartTMP"))
            {
                SceneManager.LoadScene("BaseScene");
            }
            if (name.Equals("ExitTMP"))
            {
                Application.Quit();
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventdata)
    {
        if (name.Equals("StartTMP"))
        {
            StartEffect.Play();
        }
        if (name.Equals("ExitTMP"))
        {
            ExitEffect.Play();
        }
    }
    public void OnPointerExit(PointerEventData eventdata)
    {
        if (name.Equals("StartTMP"))
        {
            StartEffect.Stop();
        }
        if (name.Equals("ExitTMP"))
        {
            ExitEffect.Stop();
        }
    }
}
