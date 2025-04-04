using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Movement", menuName = "Scriptable Objects/Movement/CharacterController"), InlineEditor]
public class GTSO_MovementParams : ScriptableObject
{
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Acceleration{ get; private set; }
    [field: SerializeField] public float Deceleration{ get; private set; }
    [field: SerializeField] public float JumpStrength { get; private set; }
    [field: SerializeField] public float GravityMultiplier { get; private set; }
}
