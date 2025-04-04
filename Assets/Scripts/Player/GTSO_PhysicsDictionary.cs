using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsDictionary", menuName = "Scriptable Objects/Movement/PhysicsDictionary"), InlineEditor]
public class GTSO_PhysicsDictionary : SerializedScriptableObject
{
    [SerializeField]
    public Dictionary<EGrabbingState, GTSO_PhysicsParams> PhysicParamsDictionary;
}
