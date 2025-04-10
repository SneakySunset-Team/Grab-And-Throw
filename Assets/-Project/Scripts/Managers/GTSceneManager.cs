using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GTSceneManager : GTSingleton<GTSceneManager>
{
    public static Action OnSceneReloadEvent;
    public void ReloadScene()
    {
        OnSceneReloadEvent?.Invoke();
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadScene(string sceneName)
    {
        OnSceneReloadEvent?.Invoke();
        SceneManager.LoadSceneAsync(sceneName);
    }
}
