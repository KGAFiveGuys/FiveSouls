using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField]public GameObject Portal_Obj;
    [SerializeField] public BoxCollider Portal_col;
    private void Start()
    {
        Portal_col = GetComponent<BoxCollider>();
        Portal_Obj.SetActive(false);
        Portal_col.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Player"))
        {
            SceneManager.LoadScene("CutScene");
        }
    }
}
