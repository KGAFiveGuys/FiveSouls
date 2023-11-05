using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthForeground;
    [SerializeField] private Slider healthBackground;
    [SerializeField] private float healthBackgroundDelay = 1f;
    [SerializeField] private float healthBackgroundLerpDuration = .5f;
    [SerializeField] private AnimationCurve healthBackgroundLerpIntensity;

    [Header("Stamina")]
    [SerializeField] private Slider staminaForeground;
    [SerializeField] private Slider staminaBackground;
    [SerializeField] private float staminaBackgroundDelay = .5f;
    [SerializeField] private float staminaBackgroundLerpDuration = .5f;
    [SerializeField] private AnimationCurve staminaBackgroundLerpIntensity;

    [Header("OnDead")]
    [SerializeField] private float onDeadBackgroundDelay = .3f;
    [SerializeField] private float onDeadBackgroundLerpDuration = .3f;
    [SerializeField] private AnimationCurve onDeadBackgroundLerpIntensity;

    private Health _health;
    private Stamina _stamina;
    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _health = playerObj.GetComponent<Health>();
        _stamina = playerObj.GetComponent<Stamina>();
    }

    private void OnEnable()
    {
        _health.OnHealthChanged += ChangeHealthUI;
        _stamina.OnStaminaChanged += ChangeStaminaUI;
        _health.OnDead += ResetHealthNStamina;
    }

    private void OnDisable()
    {
        _health.OnHealthChanged -= ChangeHealthUI;
        _stamina.OnStaminaChanged -= ChangeStaminaUI;
        _health.OnDead -= ResetHealthNStamina;
    }

    private void Start()
    {
        healthForeground.value = _health.CurrentHP / _health.MaxHP;
        healthBackground.value = healthForeground.value;

        staminaForeground.value = _stamina.CurrentStamina / _stamina.MaxStamina;
        staminaBackground.value = staminaForeground.value;

        currentCheckStamina = CheckStaminaBackground();
        StartCoroutine(currentCheckStamina);
    }
    private IEnumerator currentCheckStamina;
    private IEnumerator CheckStaminaBackground()
    {
        while (true)
        {
            if (staminaForeground.value < staminaBackground.value)
            {
                if (currentSetStamina == null)
                {
                    currentSetStamina = SetStaminaBackground();
                    StartCoroutine(currentSetStamina);
                }
            }
            yield return null;
        }
    }

    private void ResetHealthNStamina()
	{
        // ���� �������� ��� UI���� Coroutine �ߴ�
        if (currentSetHealth != null)
            StopCoroutine(currentSetHealth);
        if (currentSetStamina != null)
            StopCoroutine(currentSetStamina);
        if (currentCheckStamina != null)
            StopCoroutine(currentCheckStamina);

        // UI ����
        healthForeground.value = 0;
        staminaForeground.value = 0;
        StartCoroutine(ResetHealthNStaminaBackground());
    }
    private IEnumerator ResetHealthNStaminaBackground()
    {
        // ��� ���
        float elapsedTime = 0f;
        while (elapsedTime < onDeadBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����
        elapsedTime = 0f;
        float progress = 0f;
        float healthStart = healthBackground.value;
        float staminaStart = staminaBackground.value;
        while (elapsedTime < onDeadBackgroundLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            progress = onDeadBackgroundLerpIntensity.Evaluate(elapsedTime / onDeadBackgroundLerpDuration);
            healthBackground.value = Mathf.Lerp(healthStart, 0, progress);
            staminaBackground.value = Mathf.Lerp(staminaStart, 0, progress);
            yield return null;
        }
        healthBackground.value = 0;
        staminaBackground.value = 0;
    }

    public void ChangeHealthUI()
    {
        // ���� ������� background ó���� ���� �ǰ� �� ü�� ����
        float lastHealthForeground = healthForeground.value;

        // foreground�� ��� ����
        healthForeground.value = _health.CurrentHP / _health.MaxHP;

        // ���� background�� ���� ������̶�� �ߴ� �� ��� ó��
        if (currentSetHealth != null)
        {
            StopCoroutine(currentSetHealth);
            healthBackground.value = lastHealthForeground;
        }
        
        // background�� ������� �ƴ϶�� ������ ó��
        currentSetHealth = SetHealthBackground();
        StartCoroutine(currentSetHealth);
    }

    private IEnumerator currentSetHealth = null;
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

        currentSetHealth = null;
    }

    public void ChangeStaminaUI()
    {
        // ���� �� ���¹̳ʸ� ���� ��� �ٽ� �����Ѵ�.
        elapsedTimeAfterDecreaseStamina = 0f;

        // ���¹̳� ��ȭ��
        var delta = (_stamina.CurrentStamina / _stamina.MaxStamina) - staminaForeground.value;

        // Foreground ��� ����
        staminaForeground.value += delta;

        // Background�� �޸���/����� ���� ��ȭ���� ����� ���� ��츸 ��� ����
        if (Mathf.Abs(delta) <= 0.01)
            staminaBackground.value += delta;

        // Foreground�� Background�� ��ġ�Ǹ� �ڷ�ƾ �����Ѵ�
        if (staminaForeground.value == staminaBackground.value
            && currentSetStamina != null)
        {
            StopCoroutine(currentSetStamina);
            currentSetStamina = null;
        }

        // Foreground ���� �� ��� Background�� ���� ä���
        if (staminaForeground.value == 1)
        {
            staminaBackground.value = staminaForeground.value;
        }
    }
    
    private IEnumerator currentSetStamina;
    private float elapsedTimeAfterDecreaseStamina = 0f;
    private IEnumerator SetStaminaBackground()
    {
        float elapsedTime = 0f;
        while (elapsedTime < staminaBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTimeAfterDecreaseStamina = 0f;
        float progress = 0f;
        while (elapsedTimeAfterDecreaseStamina < staminaBackgroundLerpDuration)
        {
            elapsedTimeAfterDecreaseStamina += Time.deltaTime;
            var start = staminaBackground.value;
            var end = staminaForeground.value;
            progress = staminaBackgroundLerpIntensity.Evaluate(elapsedTimeAfterDecreaseStamina / staminaBackgroundLerpDuration);
            staminaBackground.value = Mathf.Lerp(start, end, progress);

            yield return null;
        }
        currentSetStamina = null;
    }
}
