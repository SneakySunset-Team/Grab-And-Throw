using UnityEngine;
using UnityEngine.SceneManagement;

public class GTSceneManager : GTSingleton<GTSceneManager>
{
    public void ReloadScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
