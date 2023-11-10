using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedAlarm : MonoBehaviour
{
    [SerializeField] private GameObject AlarmUI;
    [SerializeField] private float Timer;
    [SerializeField] private int MuliNum;
    [SerializeField] private Image AlarmColor;
    private float flowTime;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        //보스 AI스크립트에 알람컴포턴트 find참조하고 공격패턴 출력전에 StartCoroutine(MorningCall()); 넣으면 됩니다.
        ShowAlarm();
    }

    private void ShowAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            var YPos = playerController.LockedOnEnemy.GetComponent<BoxCollider>().center.y * playerController.LockedOnEnemy.transform.localScale.y;
            Vector3 targetPosition = new Vector3(playerController.LockedOnEnemy.transform.position.x, YPos, playerController.LockedOnEnemy.transform.position.z);
            var pos = Camera.main.WorldToScreenPoint(targetPosition);
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var scale = rectTransform.localScale;
            var width = rectTransform.rect.width;
            var height = rectTransform.rect.height;
            var xOffset = scale.x * (width / 2);
            var yOffset = scale.y * (height / 2);

            AlarmUI.transform.position = new Vector3(pos.x - xOffset, pos.y + yOffset, pos.z);
        }
    }

    public IEnumerator StorngAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            AlarmUI.SetActive(true);
            AlarmColor.color = Color.red;
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var startScale = Vector3.one;
            var endScale = Vector3.one * MuliNum;
            flowTime = 0;
            while (Timer > flowTime)
            {
                flowTime += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
                yield return null;
            }
            yield return null;
            AlarmUI.SetActive(false);
        }
    }
    public IEnumerator WeakAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            AlarmUI.SetActive(true);
            AlarmColor.color = Color.yellow;
            var rectTransform = AlarmUI.GetComponent<RectTransform>();
            var startScale = Vector3.one;
            var endScale = Vector3.one * MuliNum;
            flowTime = 0;
            while (Timer > flowTime)
            {
                flowTime += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
                yield return null;
            }
            yield return null;
            AlarmUI.SetActive(false);
        }
    }
}
