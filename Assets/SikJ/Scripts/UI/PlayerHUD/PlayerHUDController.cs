using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerHUDController : MonoBehaviour
{
    public InputAction restart;

    public GameObject playerStatusUI;
    private Vector2 playerStatusUIOrigin;
    [SerializeField] private List<Image> playerStatusUIImages = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> playerStatusUITexts = new List<TextMeshProUGUI>();

    public GameObject pocketInventoryUI;
    private Vector2 pocketInventoryUIOrigin;
    [SerializeField] private List<Image> pocketInventoryUIImages = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> pocketInventoryUITexts = new List<TextMeshProUGUI>();

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
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameRestartUI;

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
    private Health _lockedOnEnemyHealth = null;
    [Header("Enemy Died")]
    [SerializeField] private GameObject enemyDied;
    #endregion

    [Header("On PickUp Item Appeared")]
    public GameObject pickUpIcon;

    [Header("On Interactable NPC Appeared")]
    public GameObject dialogueIcon;
    
    private void Awake()
    {
		#region Player
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _playerHealth = playerObj.GetComponent<Health>();
        _playerStamina = playerObj.GetComponent<Stamina>();
        _playerController = playerObj.GetComponent<PlayerController>();
        #endregion

        playerStatusUIOrigin = playerStatusUI.GetComponent<RectTransform>().anchoredPosition;
        pocketInventoryUIOrigin = pocketInventoryUI.GetComponent<RectTransform>().anchoredPosition;
    }

	private void OnEnable()
    {
		#region Player
		_playerHealth.OnHealthChanged += ChangePlayerHealth;
        _playerStamina.OnStaminaChanged += ChangePlayerStamina;
        _playerHealth.OnDead += ShowGameOver;
        _playerHealth.OnDead += ShowRestart;
        _playerHealth.OnDead += ResetPlayerHealthNStamina;
        _playerHealth.OnDead += HideEnemyHealthNFury;
        _playerHealth.OnRevive += HideGameOver;
        _playerHealth.OnRevive += HideRestart;
        _playerHealth.OnRevive += RestoreResetPlayerHealthNStamina;
        _playerController.OnLockOn += ShowEnemyHealthNFury;
        _playerController.OnLockOff += HideEnemyHealthNFury;
        #endregion

        restart.performed += RestartGame;
        restart.Enable();
    }

	private void OnDisable()
    {
        #region Player
        _playerHealth.OnHealthChanged -= ChangePlayerHealth;
        _playerStamina.OnStaminaChanged -= ChangePlayerStamina;
        _playerHealth.OnDead -= ShowGameOver;
        _playerHealth.OnDead -= ShowRestart;
        _playerHealth.OnDead -= ResetPlayerHealthNStamina;
        _playerHealth.OnDead -= HideEnemyHealthNFury;
        _playerHealth.OnRevive -= HideGameOver;
        _playerHealth.OnRevive -= HideRestart;
        _playerHealth.OnRevive -= RestoreResetPlayerHealthNStamina;
        _playerController.OnLockOn -= ShowEnemyHealthNFury;
        _playerController.OnLockOff -= HideEnemyHealthNFury;
        #endregion

        restart.performed -= RestartGame;
        restart.Disable();
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

    #region Fade In/Out PlayerHUD
    public void FadeInPlayerHUD()
    {
        StartCoroutine(StartFadeIn());
    }

    public void FadeOutPlayerHUD()
    {
        StartCoroutine(StartFadeOut());
    }

    [Header("Fade In/Out")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeInSpeedOverTime;
    [SerializeField] private AnimationCurve fadeOutSpeedOverTime;
    private IEnumerator StartFadeIn()
    {
        var status = playerStatusUI.GetComponent<RectTransform>();
        var statusStartPos = status.anchoredPosition;
        var statusEndPos = playerStatusUIOrigin;

        var inventory = pocketInventoryUI.GetComponent<RectTransform>();
        var inventoryStartPos = inventory.anchoredPosition;
        var inventoryEndPos = pocketInventoryUIOrigin;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            var currentFadeRate = fadeInSpeedOverTime.Evaluate(elapsedTime / fadeDuration);

            // Fade In
            status.anchoredPosition = Vector2.Lerp(statusStartPos, statusEndPos, currentFadeRate);
            inventory.anchoredPosition = Vector2.Lerp(inventoryStartPos, inventoryEndPos, currentFadeRate);

            // Make Opaque
            foreach (var image in playerStatusUIImages)
            {
                var startColor = image.color;
                var endColor = new Color(image.color.r, image.color.g, image.color.b, 1);
                image.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var text in playerStatusUITexts)
            {
                var startColor = text.color;
                var endColor = new Color(text.color.r, text.color.g, text.color.b, 1);
                text.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var image in pocketInventoryUIImages)
            {
                var startColor = image.color;
                var endColor = new Color(image.color.r, image.color.g, image.color.b, 1);
                image.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var text in pocketInventoryUITexts)
            {
                var startColor = text.color;
                var endColor = new Color(text.color.r, text.color.g, text.color.b, 1);
                text.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }

            yield return null;
        }
        status.anchoredPosition = statusEndPos;
        inventory.anchoredPosition = inventoryEndPos;
    }
    private IEnumerator StartFadeOut()
    {
        var status = playerStatusUI.GetComponent<RectTransform>();
        var statusStartPos = playerStatusUIOrigin;
        var statusEndPos = status.anchoredPosition * 2;

        var inventory = pocketInventoryUI.GetComponent<RectTransform>();
        var inventoryStartPos = pocketInventoryUIOrigin;
        var inventoryEndPos = inventory.anchoredPosition * 2;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            var currentFadeRate = fadeOutSpeedOverTime.Evaluate(elapsedTime / fadeDuration);

            // Fade Out
            status.anchoredPosition = Vector2.Lerp(statusStartPos, statusEndPos, currentFadeRate);
            inventory.anchoredPosition = Vector2.Lerp(inventoryStartPos, inventoryEndPos, currentFadeRate);

            // Make Transparent
            foreach (var image in playerStatusUIImages)
            {
                var startColor = image.color;
                var endColor = new Color(image.color.r, image.color.g, image.color.b, 0);
                image.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var text in playerStatusUITexts)
            {
                var startColor = text.color;
                var endColor = new Color(text.color.r, text.color.g, text.color.b, 0);
                text.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var image in pocketInventoryUIImages)
            {
                var startColor = image.color;
                var endColor = new Color(image.color.r, image.color.g, image.color.b, 0);
                image.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }
            foreach (var text in pocketInventoryUITexts)
            {
                var startColor = text.color;
                var endColor = new Color(text.color.r, text.color.g, text.color.b, 0);
                text.color = Color.Lerp(startColor, endColor, currentFadeRate);
            }

            yield return null;
        }
        status.anchoredPosition = statusEndPos;
        inventory.anchoredPosition = inventoryEndPos;
    }
    #endregion

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

    private void RestoreResetPlayerHealthNStamina()
    {
        playerForegroundHealth.value = _playerHealth.CurrentHP / _playerHealth.MaxHP;
        playerBackgroundHealth.value = playerForegroundHealth.value;

        playerForegroundStamina.value = _playerStamina.CurrentStamina / _playerStamina.MaxStamina;
        playerBackgroundStamina.value = playerForegroundStamina.value;

        currentCheckPlayerStamina = CheckStaminaBackground();
        StartCoroutine(currentCheckPlayerStamina);
    }

    private void ResetPlayerHealthNStamina()
	{
        // 현재 진행중인 모든 UI관련 Coroutine 중단
        if (currentSetPlayerBackgroundHealth != null)
            StopCoroutine(currentSetPlayerBackgroundHealth);
        if (currentSetPlayerStamina != null)
            StopCoroutine(currentSetPlayerStamina);
        if (currentCheckPlayerStamina != null)
            StopCoroutine(currentCheckPlayerStamina);

        // UI 변경
        playerForegroundHealth.value = 0;
        playerForegroundStamina.value = 0;
        StartCoroutine(ResetPlayerHealthNStaminaBackground());
    }

    private IEnumerator ResetPlayerHealthNStaminaBackground()
    {
        // 잠시 대기
        float elapsedTime = 0f;
        while (elapsedTime < playerBackgroundDelay_OnDead)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
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
        // 현재 대기중인 background 처리를 위해 피격 전 체력 저장
        float lastHealthForeground = playerForegroundHealth.value;

        // foreground는 즉시 변경
        playerForegroundHealth.value = _playerHealth.CurrentHP / _playerHealth.MaxHP;

        // 현재 background가 변경 대기중이라면 중단 후 즉시 처리
        if (currentSetPlayerBackgroundHealth != null)
        {
            StopCoroutine(currentSetPlayerBackgroundHealth);
            playerBackgroundHealth.value = lastHealthForeground;
        }
        
        // background가 대기중이 아니라면 지연된 처리
        currentSetPlayerBackgroundHealth = SetPlayerBackgroundHealth();
        StartCoroutine(currentSetPlayerBackgroundHealth);
    }

    private IEnumerator currentSetPlayerBackgroundHealth = null;
    private IEnumerator SetPlayerBackgroundHealth()
    {
        // 잠시 대기
        float elapsedTime = 0f;
        while (elapsedTime < playerBackgroundHealthDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
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
        // 보간 중 스태미너를 쓰는 경우 다시 보간한다.
        elapsedTimeAfterDecreaseStamina = 0f;

        // 스태미너 변화량
        var delta = (_playerStamina.CurrentStamina / _playerStamina.MaxStamina) - playerForegroundStamina.value;

        // Foreground 즉시 적용
        playerForegroundStamina.value += delta;

        // Background는 달리기/재생과 같이 변화량이 충분히 작은 경우만 즉시 적용
        if (Mathf.Abs(delta) <= 0.01)
            playerBackgroundStamina.value += delta;

        // Foreground와 Background가 일치되면 코루틴 종료한다
        if (playerForegroundStamina.value == playerBackgroundStamina.value
            && currentSetPlayerStamina != null)
        {
            StopCoroutine(currentSetPlayerStamina);
            currentSetPlayerStamina = null;
        }

        // Foreground 가득 찬 경우 Background도 가득 채운다
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
    private void ShowEnemyHealthNFury()
	{
        if (!_playerController.LockedOnEnemy.TryGetComponent(out _lockedOnEnemyHealth))
            return;

        enemyForegroundHealth.value = _lockedOnEnemyHealth.CurrentHP / _lockedOnEnemyHealth.MaxHP;
        enemyBackgroundHealth.value = enemyForegroundHealth.value;

        // lockedOnEnemy의 Event 구독
        _lockedOnEnemyHealth.OnHealthChanged += ChangeEnemyHealth;
        _lockedOnEnemyHealth.OnDead += _playerController.UnlockOnPoint;
        _lockedOnEnemyHealth.OnDead += HideEnemyHealthNFury;

        enemyIndicator.SetActive(true);
    }

    private void HideEnemyHealthNFury()
    {
        if (_lockedOnEnemyHealth == null)
            return;

        // lockedOnEnemy의 Event 구독 취소
        _lockedOnEnemyHealth.OnHealthChanged -= ChangeEnemyHealth;
        _lockedOnEnemyHealth.OnDead -= _playerController.UnlockOnPoint;
        _lockedOnEnemyHealth.OnDead -= HideEnemyHealthNFury;
        _lockedOnEnemyHealth = null;

        enemyIndicator.SetActive(false);
    }

    private void ChangeEnemyHealth()
	{
        // 현재 대기중인 background 처리를 위해 피격 전 체력 저장
        float lastForegroundHealth = enemyForegroundHealth.value;

        // foreground는 즉시 변경
        enemyForegroundHealth.value = _lockedOnEnemyHealth.CurrentHP / _lockedOnEnemyHealth.MaxHP;

        // 현재 background가 변경 대기중이라면 중단 후 즉시 처리
        if (currentSetEnemyBackgroundHealth != null)
        {
            StopCoroutine(currentSetEnemyBackgroundHealth);
            enemyBackgroundHealth.value = lastForegroundHealth;
        }

        // background가 대기중이 아니라면 지연된 처리
        currentSetEnemyBackgroundHealth = SetEnemyBackgroundHealth();
        StartCoroutine(currentSetEnemyBackgroundHealth);
    }

    private IEnumerator currentSetEnemyBackgroundHealth = null;
    private IEnumerator SetEnemyBackgroundHealth()
    {
        // 잠시 대기
        float elapsedTime = 0f;
        while (elapsedTime < enemyBackgroundHealthDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 서서히 감소
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

    private void RestartGame(InputAction.CallbackContext context)
	{
        if (!_playerController.IsDead)
            return;

        _playerHealth.Revive();
	}

    private void ShowGameOver()
    {
        gameOverUI.SetActive(true);
    }

    private void HideGameOver()
	{
        gameOverUI.SetActive(false);
    }

    private void ShowRestart()
    {
        gameRestartUI.SetActive(true);
    }

    private void HideRestart()
    {
        gameRestartUI.SetActive(false);
    }

    public void ShowEnemyDied()
	{
        enemyDied.SetActive(true);
        StartCoroutine(HideEnemyDied(2f));
	}

    private IEnumerator HideEnemyDied(float delay)
	{
        yield return new WaitForSeconds(delay);
        enemyDied.SetActive(false);
	}
}
