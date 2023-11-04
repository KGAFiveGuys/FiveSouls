using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
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

    private PlayerController _controller;
    private Health _health;
    private Stamina _stamina;
    private void Awake()
    {
        TryGetComponent(out _controller);
        TryGetComponent(out _health);
        TryGetComponent(out _stamina);
    }

    private void OnEnable()
    {
        _health.OnHealthChanged += SetHealthUI;
        _stamina.OnStaminaChanged += SetStaminaUI;
    }

    private void OnDisable()
    {
        _health.OnHealthChanged -= SetHealthUI;
        _stamina.OnStaminaChanged -= SetStaminaUI;
    }

    private void Start()
    {
        healthForeground.value = _health.CurrentHP / _health.MaxHP;
        healthBackground.value = healthForeground.value;

        staminaForeground.value = _stamina.CurrentStamina / _stamina.MaxStamina;
        staminaBackground.value = staminaForeground.value;

        StartCoroutine(CheckStaminaBackground());

        _health.OnDead += () =>
        {
            _stamina.OnStaminaChanged -= SetStaminaUI;
            staminaForeground.value = 0;
            StartCoroutine(ResetStaminaBackground());
        };
    }
    private IEnumerator ResetStaminaBackground()
    {
        float elapsedTime = 0f;
        while (elapsedTime < healthBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
        elapsedTime = 0f;
        float progress = 0f;
        float startValue = staminaBackground.value;
        float endValue = staminaForeground.value;
        while (elapsedTime < healthBackgroundLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            progress = healthBackgroundLerpIntensity.Evaluate(elapsedTime / healthBackgroundLerpDuration);
            staminaBackground.value = Mathf.Lerp(startValue, endValue, progress);
            yield return null;
        }
        staminaBackground.value = endValue;
    }

    private IEnumerator lastHealthBackground = null;
    public void SetHealthUI()
    {
        // 현재 대기중인 background 처리를 위해 피격 전 체력 저장
        float lastHealthForeground = healthForeground.value;

        // foreground는 즉시 변경
        healthForeground.value = _health.CurrentHP / _health.MaxHP;

        // 현재 background가 변경 대기중이라면 중단 후 즉시 처리
        if (lastHealthBackground != null)
        {
            StopCoroutine(lastHealthBackground);
            healthBackground.value = lastHealthForeground;
        }
        
        // background가 대기중이 아니라면 지연된 처리
        lastHealthBackground = SetHealthBackground();
        StartCoroutine(lastHealthBackground);
    }
    
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

        lastHealthBackground = null;
    }

    public void SetStaminaUI()
    {
        // 스태미너 변화량
        var delta = (_stamina.CurrentStamina / _stamina.MaxStamina) - staminaForeground.value;

        // Foreground 즉시 적용
        staminaForeground.value += delta;

        // Background는 달리기/재생과 같이 변화량이 충분히 작은 경우만 즉시 적용
        if (Mathf.Abs(delta) <= 0.01)
            staminaBackground.value += delta;

        // Foreground와 Background가 일치되면 코루틴 종료한다
        if (staminaForeground.value == staminaBackground.value
            && lastLerp != null)
        {
            StopCoroutine(lastLerp);
            lastLerp = null;
            Debug.Log("StopLerp");
        }

        // Foreground 가득 찬 경우 Background도 가득 채운다
        if (staminaForeground.value == 1)
        {
            staminaBackground.value = staminaForeground.value;
        }
    }

    private IEnumerator CheckStaminaBackground()
    {
        while (true)
        {
            if (staminaForeground.value < staminaBackground.value)
            {
                if (lastLerp == null)
                {
                    lastLerp = SetStaminaBackground();
                    StartCoroutine(lastLerp);
                    Debug.Log("StartLerp");
                }
            }
            yield return null;
        }
    }
    private IEnumerator lastLerp;
    private IEnumerator SetStaminaBackground()
    {
        float elapsedTime = 0f;
        while (elapsedTime < staminaBackgroundDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        float progress = 0f;
        while (elapsedTime < staminaBackgroundLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            var start = staminaBackground.value;
            var end = staminaForeground.value;
            progress = staminaBackgroundLerpIntensity.Evaluate(elapsedTime / staminaBackgroundLerpDuration);
            staminaBackground.value = Mathf.Lerp(start, end, progress);

            yield return null;
        }
        lastLerp = null;
    }
}
