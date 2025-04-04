using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrowSettings", menuName = "Scriptable Objects/ThrowSettings"), InlineEditor]
public class GTSO_ThrowSettings : ScriptableObject
{
    [field: SerializeField] public float MinThrowPower { get; private set; }
    [field: SerializeField] public float MaxThrowPower { get; private set; }
    [field: SerializeField] public ThrowType ThrowType { get; private set; }
    [field: SerializeField] public float MaxThrowChargeTime { get; private set; }
    [field: SerializeField, Range(0, 90)] public float ThrowAngle { get; private set; }
    [field: SerializeField] public bool IsMovementDisabled { get; private set; }
}

public enum ThrowType
{
    PressToThrow, // Throw regardless of holding time
    ChargeXTimeToThrow, // Hold X time the key button and throw automatically the object
    ChargeUntilMaxValueToThrow, // Hold X time the key button, increase throwing power to a maximum value
    ChargeFreeToThrow // Hold the button for as long as you like, throwing power increases and decreases sinusoidally.
}
