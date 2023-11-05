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
        // 현재 진행중인 모든 UI관련 Coroutine 중단
        if (currentSetHealth != null)
            StopCoroutine(currentSetHealth);
        if (currentSetStamina != null)
            StopCoroutine(currentSetStamina);
        if (currentCheckStamina != null)
            StopCoroutine(currentCheckStamina);

        // UI 변경
        healthForeground.value = 0;
        staminaForeground.value = 0;
        StartCoroutine(ResetHealthNStaminaBackground());
    }
    private IEnumerator ResetHealthNStaminaBackground()
    {
        // 잠시 대기
        float elapsedTime = 0f;
        while (elapsedTime < onDeadBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
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
        // 현재 대기중인 background 처리를 위해 피격 전 체력 저장
        float lastHealthForeground = healthForeground.value;

        // foreground는 즉시 변경
        healthForeground.value = _health.CurrentHP / _health.MaxHP;

        // 현재 background가 변경 대기중이라면 중단 후 즉시 처리
        if (currentSetHealth != null)
        {
            StopCoroutine(currentSetHealth);
            healthBackground.value = lastHealthForeground;
        }
        
        // background가 대기중이 아니라면 지연된 처리
        currentSetHealth = SetHealthBackground();
        StartCoroutine(currentSetHealth);
    }

    private IEnumerator currentSetHealth = null;
    private IEnumerator SetHealthBackground()
    {
        // 잠시 대기
        float elapsedTime = 0f;
        while (elapsedTime < healthBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
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
        // 보간 중 스태미너를 쓰는 경우 다시 보간한다.
        elapsedTimeAfterDecreaseStamina = 0f;

        // 스태미너 변화량
        var delta = (_stamina.CurrentStamina / _stamina.MaxStamina) - staminaForeground.value;

        // Foreground 즉시 적용
        staminaForeground.value += delta;

        // Background는 달리기/재생과 같이 변화량이 충분히 작은 경우만 즉시 적용
        if (Mathf.Abs(delta) <= 0.01)
            staminaBackground.value += delta;

        // Foreground와 Background가 일치되면 코루틴 종료한다
        if (staminaForeground.value == staminaBackground.value
            && currentSetStamina != null)
        {
            StopCoroutine(currentSetStamina);
            currentSetStamina = null;
        }

        // Foreground 가득 찬 경우 Background도 가득 채운다
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
