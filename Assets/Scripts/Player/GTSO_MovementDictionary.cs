using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static GTCharacter_Move;

[CreateAssetMenu(fileName = "MovementDictionary", menuName = "Scriptable Objects/Movement/MovementDictionary"), InlineEditor]
public class GTSO_MovementDictionary : SerializedScriptableObject
{
    [SerializeField]
    public Dictionary<EMovementState, GTSO_MovementParams> MovementParamsDictionary;
}
