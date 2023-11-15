using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicTimer : MonoBehaviour
{
    public Gradient gradient;
    [SerializeField] private float timer;
    [SerializeField] private float Speed = 2;
    [SerializeField] private Image MagicImage;
    [SerializeField] private WizardControl wizard;
    // Start is called before the first frame update
    void Start()
    {
        MagicImage = transform.GetComponent<Image>();
        wizard = FindObjectOfType<WizardControl>();
        MagicImage.fillAmount = 0;
        timer = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if(!wizard.wizardinfo.status.Equals(Status.MegaPattern))
        {
            timer += Speed *Time.deltaTime / 100;
            MagicImage.fillAmount = timer;
        }
        MagicImage.color = gradient.Evaluate(MagicImage.fillAmount);
        if (MagicImage.fillAmount == 1 && !wizard.wizardinfo.status.Equals(Status.MegaPattern))
        {
            SetMegaPattern();
        }
    }
    private void SetMegaPattern()
    {
        wizard.wizardinfo.status = Status.MegaPattern;
        StartCoroutine(wizard.megapattern.MegaThunderPatternUse());
        MagicImage.fillAmount = 0;
        timer = 0;
    }
}
