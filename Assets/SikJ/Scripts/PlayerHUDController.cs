using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
public class PlayerHUDController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private float healthBackgroundDelay = 1f;
    [SerializeField] private float healthBackgroundLerpDuration = .5f;
    [SerializeField] private AnimationCurve healthBackgroundLerpIntensity;

    [SerializeField] private Slider healthForeground;
    [SerializeField] private Slider healthBackground;
    [SerializeField] private Slider staminaForeground;
    [SerializeField] private Slider staminaBackground;

    private Health playerHealth;
    private Stamina playerStamina;
    private void Awake()
    {
        TryGetComponent(out playerHealth);
        TryGetComponent(out playerStamina);
    }

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += SetHealthUI;
        playerStamina.OnStaminaChanged += SetStaminaUI;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= SetHealthUI;
        playerStamina.OnStaminaChanged -= SetStaminaUI;
    }

    private void Start()
    {
        healthForeground.value = playerHealth.CurrentHP / playerHealth.MaxHP;
        healthBackground.value = playerHealth.CurrentHP / playerHealth.MaxHP;

        staminaForeground.value = playerStamina.CurrentStamina / playerStamina.MaxStamina;
        staminaForeground.value = playerStamina.CurrentStamina / playerStamina.MaxStamina;
    }

    private IEnumerator lastHealthBackground = null;
    public void SetHealthUI()
    {
        // ���� ������� background ó���� ���� �ǰ� �� ü�� ����
        float lastHealthForeground = healthForeground.value;

        // foreground�� ��� ����
        healthForeground.value = playerHealth.CurrentHP / playerHealth.MaxHP;

        // ���� background�� ���� ������̶�� �ߴ� �� ��� ó��
        if (lastHealthBackground != null)
        {
            StopCoroutine(lastHealthBackground);
            healthBackground.value = lastHealthForeground;
        }
        
        // background�� ������� �ƴ϶�� ������ ó��
        lastHealthBackground = SetHealthBackground();
        StartCoroutine(lastHealthBackground);
    }
    
    private IEnumerator SetHealthBackground()
    {
        // ��� ���
        float elapsedTime = 0f;
        while (elapsedTime < healthBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����
        elapsedTime = 0f;
        float progress = 0f;
        float startValue = healthBackground.value;
        float endValue = healthForeground.value;
        while (elapsedTime < healthBackgroundLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            progress = healthBackgroundLerpIntensity.Evaluate(elapsedTime / healthBackgroundLerpDuration);
            healthBackground.value = Mathf.Lerp(startValue, endValue, progress);
            yield return null;
        }
        healthBackground.value = endValue;

        lastHealthBackground = null;
    }

    public void SetStaminaUI()
    {
        // foreground�� ��� ����
        staminaForeground.value = playerStamina.CurrentStamina / playerStamina.MaxStamina;
    }
}
