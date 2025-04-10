using System;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

public class GTCheatManager : GTSingleton<GTCheatManager>
{
    [SerializeField] TextMeshProUGUI _spawnPointIdTxt;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangePlayerSpawnIndex();
        }
    }

    private void ChangePlayerSpawnIndex()
    {
        int maxIndex = 0;
        FindObjectsByType<GTPlayerStart>(FindObjectsInactive.Include, FindObjectsSortMode.None).ForEach(a=>
        {
            if( a.PlayerStartIndex > maxIndex)
                maxIndex = a.PlayerStartIndex;
        });
        GTPlayerManager.Instance.SetPlayerStartIndex(((GTPlayerManager.Instance.GetPlayerStartIndex() + 1) % (maxIndex + 1)));
        _spawnPointIdTxt.text = $"Respawn Id : {GTPlayerManager.Instance.GetPlayerStartIndex()}";
    }
}
