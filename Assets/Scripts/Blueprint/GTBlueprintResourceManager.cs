using System.Collections.Generic;
using UnityEngine;

public class GTBlueprintResourceManager : GTSingleton<GTBlueprintResourceManager>
{
    // ****** PUBLIC      *****************************************

    public Sprite GetComponentIcon(EComponentType componentId)
    {
        if (_itemIcons.ContainsKey(componentId))
            return _itemIcons[componentId];

        return null;
    }


    // ****** UNITY      ******************************************

    [SerializeField] private Dictionary<EComponentType, Sprite> _itemIcons = new Dictionary<EComponentType, Sprite>();


}
