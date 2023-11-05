using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
	#region Player
	[Header("Player Health")]
    [SerializeField] private Slider playerForegroundHealth;
    [SerializeField] private Slider playerBackgroundHealth;
    [SerializeField] private float playerBackgroundHealthDelay = 1f;
    [SerializeField] private float playerBackgroundHealthLerpDuration = .5f;
    [SerializeField] private AnimationCurve playerBackgroundHealthLerpIntensity;

    [Header("Player Stamina")]
    [SerializeField] private Slider playerForegroundStamina;
    [SerializeField] private Slider playerBackgroundStamina;
    [SerializeField] private float playerBackgroundStaminaDelay = .5f;
    [SerializeField] private float playerBackgroundStaminaLerpDuration = .5f;
    [SerializeField] private AnimationCurve playerBackgroundStaminaLerpIntensity;

    [Header("On Player Dead")]
    [SerializeField] private float playerBackgroundDelay_OnDead = .3f;
    [SerializeField] private float playerBackgroundLerpDuration_OnDead = .3f;
    [SerializeField] private AnimationCurve playerBackgroundLerpIntensity_OnDead;

    private Health _playerHealth;
    private Stamina _playerStamina;
    private PlayerController _playerController;
    #endregion
    #region LockedOnEnemy
    [Header("Enemy Indicator")]
    [SerializeField] private GameObject enemyIndicator;
    [Header("Enemy Health")]
    [SerializeField] private Slider enemyForegroundHealth;
    [SerializeField] private Slider enemyBackgroundHealth;
    [SerializeField] private float enemyBackgroundHealthDelay = 1f;
    [SerializeField] private float enemyBackgroundHealthLerpDuration = .5f;
    [SerializeField] private AnimationCurve enemyBackgroundHealthLerpIntensity;
    [Header("Enemy Fury")]

    private GameObject _lockedOnEnemy = null;
    private Health _lockedOnEnemyHealth = null;
    #endregion

    private void Awake()
    {
		#region Player
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _playerHealth = playerObj.GetComponent<Health>();
        _playerStamina = playerObj.GetComponent<Stamina>();
        _playerController = playerObj.GetComponent<PlayerController>();
        #endregion
    }

	private void OnEnable()
    {
		#region Player
		_playerHealth.OnHealthChanged += ChangePlayerHealth;
        _playerHealth.OnDead += ResetPlayerHealthNStamina;
        _playerHealth.OnDead += HideEnemyHealthNFury;
        _playerStamina.OnStaminaChanged += ChangePlayerStamina;
        _playerController.OnLockOn += ShowEnemyHealthNFury;
        _playerController.OnLockOff += HideEnemyHealthNFury;
        #endregion
    }

	private void OnDisable()
    {
        #region Player
        _playerHealth.OnHealthChanged -= ChangePlayerHealth;
        _playerStamina.OnStaminaChanged -= ChangePlayerStamina;
        _playerHealth.OnDead -= ResetPlayerHealthNStamina;
        _playerHealth.OnDead -= HideEnemyHealthNFury;
        _playerController.OnLockOn -= ShowEnemyHealthNFury;
        _playerController.OnLockOff -= HideEnemyHealthNFury;
        #endregion
    }

	private void Start()
    {
		#region Player
		playerForegroundHealth.value = _playerHealth.CurrentHP / _playerHealth.MaxHP;
        playerBackgroundHealth.value = playerForegroundHealth.value;
        playerForegroundStamina.value = _playerStamina.CurrentStamina / _playerStamina.MaxStamina;
        playerBackgroundStamina.value = playerForegroundStamina.value;
        currentCheckPlayerStamina = CheckStaminaBackground();
        StartCoroutine(currentCheckPlayerStamina);
		#endregion
	}

	#region Player
	private IEnumerator currentCheckPlayerStamina;
    private IEnumerator CheckStaminaBackground()
    {
        while (true)
        {
            if (playerForegroundStamina.value < playerBackgroundStamina.value)
            {
                if (currentSetPlayerStamina == null)
                {
                    currentSetPlayerStamina = SetStaminaBackground();
                    StartCoroutine(currentSetPlayerStamina);
                }
            }
            yield return null;
        }
    }

    private void ResetPlayerHealthNStamina()
	{
        // ���� �������� ��� UI���� Coroutine �ߴ�
        if (currentSetPlayerBackgroundHealth != null)
            StopCoroutine(currentSetPlayerBackgroundHealth);
        if (currentSetPlayerStamina != null)
            StopCoroutine(currentSetPlayerStamina);
        if (currentCheckPlayerStamina != null)
            StopCoroutine(currentCheckPlayerStamina);

        // UI ����
        playerForegroundHealth.value = 0;
        playerForegroundStamina.value = 0;
        StartCoroutine(ResetPlayerHealthNStaminaBackground());
    }

    private IEnumerator ResetPlayerHealthNStaminaBackground()
    {
        // ��� ���
        float elapsedTime = 0f;
        while (elapsedTime < playerBackgroundDelay_OnDead)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����
        elapsedTime = 0f;
        float progress = 0f;
        float healthStart = playerBackgroundHealth.value;
        float staminaStart = playerBackgroundStamina.value;
        while (elapsedTime < playerBackgroundLerpDuration_OnDead)
        {
            elapsedTime += Time.deltaTime;
            progress = playerBackgroundLerpIntensity_OnDead.Evaluate(elapsedTime / playerBackgroundLerpDuration_OnDead);
            playerBackgroundHealth.value = Mathf.Lerp(healthStart, 0, progress);
            playerBackgroundStamina.value = Mathf.Lerp(staminaStart, 0, progress);
            yield return null;
        }
        playerBackgroundHealth.value = 0;
        playerBackgroundStamina.value = 0;
    }

    public void ChangePlayerHealth()
    {
        // ���� ������� background ó���� ���� �ǰ� �� ü�� ����
        float lastHealthForeground = playerForegroundHealth.value;

        // foreground�� ��� ����
        playerForegroundHealth.value = _playerHealth.CurrentHP / _playerHealth.MaxHP;

        // ���� background�� ���� ������̶�� �ߴ� �� ��� ó��
        if (currentSetPlayerBackgroundHealth != null)
        {
            StopCoroutine(currentSetPlayerBackgroundHealth);
            playerBackgroundHealth.value = lastHealthForeground;
        }
        
        // background�� ������� �ƴ϶�� ������ ó��
        currentSetPlayerBackgroundHealth = SetPlayerBackgroundHealth();
        StartCoroutine(currentSetPlayerBackgroundHealth);
    }

    private IEnumerator currentSetPlayerBackgroundHealth = null;
    private IEnumerator SetPlayerBackgroundHealth()
    {
        // ��� ���
        float elapsedTime = 0f;
        while (elapsedTime < playerBackgroundHealthDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����
        elapsedTime = 0f;
        float progress = 0f;
        float startValue = playerBackgroundHealth.value;
        float endValue = playerForegroundHealth.value;
        while (elapsedTime < playerBackgroundHealthLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            progress = playerBackgroundHealthLerpIntensity.Evaluate(elapsedTime / playerBackgroundHealthLerpDuration);
            playerBackgroundHealth.value = Mathf.Lerp(startValue, endValue, progress);
            yield return null;
        }
        playerBackgroundHealth.value = endValue;

        currentSetPlayerBackgroundHealth = null;
    }

    public void ChangePlayerStamina()
    {
        // ���� �� ���¹̳ʸ� ���� ��� �ٽ� �����Ѵ�.
        elapsedTimeAfterDecreaseStamina = 0f;

        // ���¹̳� ��ȭ��
        var delta = (_playerStamina.CurrentStamina / _playerStamina.MaxStamina) - playerForegroundStamina.value;

        // Foreground ��� ����
        playerForegroundStamina.value += delta;

        // Background�� �޸���/����� ���� ��ȭ���� ����� ���� ��츸 ��� ����
        if (Mathf.Abs(delta) <= 0.01)
            playerBackgroundStamina.value += delta;

        // Foreground�� Background�� ��ġ�Ǹ� �ڷ�ƾ �����Ѵ�
        if (playerForegroundStamina.value == playerBackgroundStamina.value
            && currentSetPlayerStamina != null)
        {
            StopCoroutine(currentSetPlayerStamina);
            currentSetPlayerStamina = null;
        }

        // Foreground ���� �� ��� Background�� ���� ä���
        if (playerForegroundStamina.value == 1)
        {
            playerBackgroundStamina.value = playerForegroundStamina.value;
        }
    }
    
    private IEnumerator currentSetPlayerStamina;
    private float elapsedTimeAfterDecreaseStamina = 0f;
    private IEnumerator SetStaminaBackground()
    {
        float elapsedTime = 0f;
        while (elapsedTime < playerBackgroundStaminaDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTimeAfterDecreaseStamina = 0f;
        float progress = 0f;
        while (elapsedTimeAfterDecreaseStamina < playerBackgroundStaminaLerpDuration)
        {
            elapsedTimeAfterDecreaseStamina += Time.deltaTime;
            var start = playerBackgroundStamina.value;
            var end = playerForegroundStamina.value;
            progress = playerBackgroundStaminaLerpIntensity.Evaluate(elapsedTimeAfterDecreaseStamina / playerBackgroundStaminaLerpDuration);
            playerBackgroundStamina.value = Mathf.Lerp(start, end, progress);

            yield return null;
        }
        currentSetPlayerStamina = null;
    }
    #endregion
    #region LockedOnEnemy
    private void ShowEnemyHealthNFury(GameObject lockedOnEnemy)
	{
        _lockedOnEnemy = lockedOnEnemy;
        _lockedOnEnemyHealth = _lockedOnEnemy.GetComponent<Health>();
        enemyForegroundHealth.value = _lockedOnEnemyHealth.CurrentHP / _lockedOnEnemyHealth.MaxHP;
        enemyBackgroundHealth.value = enemyForegroundHealth.value;

        // lockedOnEnemy�� Event ����
        _lockedOnEnemyHealth.OnHealthChanged += ChangeEnemyHealth;
        _lockedOnEnemyHealth.OnDead += HideEnemyHealthNFury;

        enemyIndicator.SetActive(true);
    }

    private void HideEnemyHealthNFury()
    {
        if (_lockedOnEnemy == null)
            return;

        _lockedOnEnemy = null;

        // lockedOnEnemy�� Event ���� ���
        _lockedOnEnemyHealth.OnHealthChanged -= ChangeEnemyHealth;
        _lockedOnEnemyHealth.OnDead -= HideEnemyHealthNFury;
        _lockedOnEnemyHealth = null;

        enemyIndicator.SetActive(false);
    }

    private void ChangeEnemyHealth()
	{
        // ���� ������� background ó���� ���� �ǰ� �� ü�� ����
        float lastForegroundHealth = enemyForegroundHealth.value;

        // foreground�� ��� ����
        enemyForegroundHealth.value = _lockedOnEnemyHealth.CurrentHP / _lockedOnEnemyHealth.MaxHP;

        // ���� background�� ���� ������̶�� �ߴ� �� ��� ó��
        if (currentSetEnemyBackgroundHealth != null)
        {
            StopCoroutine(currentSetEnemyBackgroundHealth);
            enemyBackgroundHealth.value = lastForegroundHealth;
        }

        // background�� ������� �ƴ϶�� ������ ó��
        currentSetEnemyBackgroundHealth = SetEnemyBackgroundHealth();
        StartCoroutine(currentSetEnemyBackgroundHealth);
    }

    private IEnumerator currentSetEnemyBackgroundHealth = null;
    private IEnumerator SetEnemyBackgroundHealth()
    {
        // ��� ���
        float elapsedTime = 0f;
        while (elapsedTime < enemyBackgroundHealthDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����
        elapsedTime = 0f;
        float progress = 0f;
        float startValue = enemyBackgroundHealth.value;
        float endValue = enemyForegroundHealth.value;
        while (elapsedTime < enemyBackgroundHealthLerpDuration)
        {
            elapsedTime += Time.deltaTime;
            progress = enemyBackgroundHealthLerpIntensity.Evaluate(elapsedTime / enemyBackgroundHealthLerpDuration);
            enemyBackgroundHealth.value = Mathf.Lerp(startValue, endValue, progress);
            yield return null;
        }
        enemyBackgroundHealth.value = endValue;

        currentSetEnemyBackgroundHealth = null;
    }
    #endregion
}
