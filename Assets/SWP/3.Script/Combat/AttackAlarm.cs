using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackAlarm : MonoBehaviour
{
    [Header("알람설정")]
    [SerializeField] private GameObject AlarmUI;
    [SerializeField] private Image AlarmColor;
    [SerializeField] private float Timer;
    [SerializeField] private int MultiNum;
    private PlayerController playerController;
    private float flowTime;
    private float Acolor;

    public static AttackAlarm Instance = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        ShowAlarm();
    }

    private void ShowAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            var pos = Camera.main.WorldToScreenPoint(playerController.LockOnTargetPoint.transform.position);
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var scale = rectTransform.localScale;
            var width = rectTransform.rect.width;
            var height = rectTransform.rect.height;
            var xOffset = scale.x * (width / 2);
            var yOffset = scale.y * (height / 2);

            AlarmUI.transform.position = new Vector3(pos.x - xOffset, pos.y + yOffset, pos.z);
        }
    }

    public void RedAlarm()
    {
        StartCoroutine(StrongAlarm());
    }
    public void YellowAlarm()
    {
        StartCoroutine(WeakAlarm());
    }

    private IEnumerator StrongAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            Acolor = 255f;
            AlarmUI.SetActive(true);
            AlarmColor.color = new Color(255, 0, 0, Acolor);
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var startScale = Vector3.one;
            var endScale = Vector3.one * MultiNum;
            flowTime = 0;
            while (Timer > flowTime)
            {
                flowTime += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
                Acolor -= 40f;
                AlarmColor.color = new Color(255, 0, 0, Acolor);

                yield return null;
            }
            yield return null;
            AlarmUI.SetActive(false);
        }
    }

    private IEnumerator WeakAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            Acolor = 255f;
            AlarmUI.SetActive(true);
            AlarmColor.color = new Color(255, 255, 0, Acolor);
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var startScale = Vector3.one;
            var endScale = Vector3.one * MultiNum;
            flowTime = 0;
            while (Timer > flowTime)
            {
                flowTime += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
                Acolor -= 40f;
                AlarmColor.color = new Color(255, 255, 0, Acolor);
                yield return null;
            }
            yield return null;
            AlarmUI.SetActive(false);
        }
    }
}
