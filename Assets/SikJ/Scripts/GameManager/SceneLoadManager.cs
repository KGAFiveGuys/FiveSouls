using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private static SceneLoadManager _instance = null;
    public static SceneLoadManager Instance => _instance;

    public int CurrentSceneIndex { get; set; } = 3;

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

    private void Start()
    {
        SceneManager.LoadScene(CurrentSceneIndex, LoadSceneMode.Additive);
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(CurrentSceneIndex++);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(CurrentSceneIndex, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
