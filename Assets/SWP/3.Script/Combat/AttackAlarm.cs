using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackAlarm : MonoBehaviour
{
    [Header("알람설정")]
    [SerializeField] private GameObject AlarmUI;
    [SerializeField] private ParticleSystem Circle;
    [SerializeField] private ParticleSystem Smoke;
    private PlayerController playerController;
    //[SerializeField] private Image AlarmColor;
    //[SerializeField] private float Timer;
    //[SerializeField] private int MultiNum;
    //private float flowTime;

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
        if (Input.GetKey(KeyCode.F3))
        {
            RedAlarm();
        }
    }

    private void ShowAlarm()
    {
        if (playerController.LockedOnEnemy != null)
        {
            var lockOnPos = playerController.LockOnTargetPoint.transform.position;
            AlarmUI.transform.position = new Vector3(lockOnPos.x, lockOnPos.y, lockOnPos.z);
            var playerPos = playerController.gameObject.transform.position;
            AlarmUI.transform.LookAt(new Vector3(playerPos.x, transform.position.y, playerPos.z));
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
        //if (playerController.LockedOnEnemy != null)
        //{
        //    AlarmUI.SetActive(true);
        //    AlarmColor.color = new Color(255, 0, 0, Acolor);
        //    var rectTransform = AlarmUI.GetComponent<RectTransform>();
        //    var startScale = Vector3.one;
        //    var endScale = Vector3.one * MultiNum;
        //    flowTime = 0;
        //    while (Timer > flowTime)
        //    {
        //        flowTime += Time.deltaTime;
        //        rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
        //        AlarmColor.color = new Color(255, 0, 0, Acolor);

        //        yield return null;
        //    }
        //    yield return null;
        //    AlarmUI.SetActive(false);
        //}
        if (playerController.LockedOnEnemy != null)
        {
            if (!Circle.isPlaying)
            {
                Circle.startColor = new Color32(125, 0, 0, 255);
                Smoke.startColor = new Color32(255, 45, 45, 255);
                yield return null;
                Circle.Play();
                Smoke.Play();
            }
        }
    }

    private IEnumerator WeakAlarm()
    {
        //if (playerController.LockedOnEnemy != null)
        //{
        //    Acolor = 255f;
        //    AlarmUI.SetActive(true);
        //    AlarmColor.color = new Color(255, 255, 0, Acolor);
        //    var rectTransform = AlarmUI.GetComponent<RectTransform>();
        //    var startScale = Vector3.one;
        //    var endScale = Vector3.one * MultiNum;
        //    flowTime = 0;
        //    while (Timer > flowTime)
        //    {
        //        flowTime += Time.deltaTime;
        //        rectTransform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
        //        Acolor -= 40f;
        //        AlarmColor.color = new Color(255, 255, 0, Acolor);
        //        yield return null;
        //    }
        //    yield return null;
        //    AlarmUI.SetActive(false);
        //}
        if (playerController.LockedOnEnemy != null)
        {
            if (!Circle.isPlaying)
            {
                Circle.startColor = new Color32(125, 125, 0, 255);
                Smoke.startColor = new Color32(255, 255, 45, 255);
                yield return null;
                Circle.Play();
                Smoke.Play();
            }
        }
    }
}
