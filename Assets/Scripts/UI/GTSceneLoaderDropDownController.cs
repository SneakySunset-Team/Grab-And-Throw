using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class GTSceneLoaderDropDownController : MonoBehaviour
{
    TMP_Dropdown _dropdown;
    bool _isFirstLoadDone = false;
    string _firstSceneName;
    private void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.ClearOptions();

        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        List<string> sceneNames = new List<string>();

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            sceneNames.Add(sceneName);
        }

        _dropdown.AddOptions(sceneNames);
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _firstSceneName = currentSceneName;
        _dropdown.value = sceneNames.IndexOf(currentSceneName);
        _dropdown.onValueChanged.AddListener(SceneChange);
    }

    public void SceneChange(int newSceneIndex)
    {
        string newSceneName = _dropdown.options[newSceneIndex].text;
        GTSceneManager.Instance.LoadScene(newSceneName);
        if (!_isFirstLoadDone)
        {

            var firstSceneItem = _dropdown.options.First(x => x.text == _firstSceneName);
            _dropdown.options.Remove(firstSceneItem);
            _isFirstLoadDone = true;
        }
    }
}
