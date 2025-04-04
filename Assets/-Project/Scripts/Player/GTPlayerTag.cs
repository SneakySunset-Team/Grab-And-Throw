using System.Collections.Generic;
using UnityEngine;


public enum EPlayerTag
{
    CharaMathis,
    CharaAndre,
    CharaThomas,
    CharaFlorian
}
public class GTPlayerTag : MonoBehaviour
{
    [SerializeField] private EPlayerTag _playerTag;

    public EPlayerTag GetPlayerTag() => _playerTag;
}