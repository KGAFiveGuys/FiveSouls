using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private static SceneLoadManager _instance = null;
    public static SceneLoadManager Instance { get; set; }

    public int CurrentSceneIndex { get; set; } = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    public void LoadNextScene()
    {
        SceneManager.UnloadSceneAsync(CurrentSceneIndex++);
        SceneManager.LoadSceneAsync(CurrentSceneIndex, LoadSceneMode.Additive);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            LoadNextScene();
        }
    }
}
