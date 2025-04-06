using System;
using Sirenix.Utilities;
using UnityEngine;

public class GTCheatManager : GTSingleton<GTCheatManager>
{
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
    }
}
