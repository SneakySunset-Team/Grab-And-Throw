using Sirenix.OdinInspector;
using System;
using System.Data;
using UnityEngine;
using static GTSO_PhysicsParams;

[CreateAssetMenu(fileName = "Physics", menuName = "Scriptable Objects/Movement/Physics"), InlineEditor]
public class GTSO_PhysicsParams : ScriptableObject
{
    [field: SerializeField, Tooltip("Is Rigidbody Active")] public bool IsPhysicsEnabled { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("Is Rigidbody Gravity Active")] public bool IsPhysicsGravityEnabled { get; private set; }
    [field: SerializeField, Tooltip("Weight of Object")] public float Mass { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("How Fast will the object fall")] public float GravityAdditionalStrength { get; private set; }

    [field: SerializeField, Tooltip("Physics Material on the Collider of the object")] public PhysicsMaterial PhysicsMaterial { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("RigidbodyConstraints")] public ECustomPhysicsConstraints RigidbodyConstraints { get; private set; }
    [field: SerializeField, Tooltip("Rigidbody Exclude Layers")] public LayerMask PhysicsEcludeLayers { get; private set; }
    //[field: SerializeField, Tooltip("How much do I push Physics objects on contact")] public float PushStrengthPhysics {  get; private set; }
    //[field: SerializeField, Tooltip("How much do I push CharacterController objects on contact")] public float PushStrengthController { get; private set; }
}

public struct GTPhysicsParams
{
    [field: SerializeField, Tooltip("Is Rigidbody Active")] public bool IsPhysicsEnabled { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("Is Rigidbody Gravity Active")] public bool IsPhysicsGravityEnabled { get; private set; }
    [field: SerializeField, Tooltip("Weight of Object")] public float Mass { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("How Fast will the object fall")] public float GravityAdditionalStrength { get; private set; }

    [field: SerializeField, Tooltip("Physics Material on the Collider of the object")] public PhysicsMaterial PhysicsMaterial { get; private set; }
    [field: SerializeField, ShowIf("IsPhysicsEnabled"), Tooltip("RigidbodyConstraints")] public ECustomPhysicsConstraints RigidbodyConstraints { get; private set; }
    [field: SerializeField, Tooltip("Rigidbody Exclude Layers")] public LayerMask PhysicsEcludeLayers { get; private set; }
    //[field: SerializeField, Tooltip("How much do I push Physics objects on contact")] public float PushStrengthPhysics {  get; private set; }
    //[field: SerializeField, Tooltip("How much do I push CharacterController objects on contact")] public float PushStrengthController { get; private set; }

    public void SetParams(GTSO_PhysicsParams physicsParams)
    {
        IsPhysicsEnabled = physicsParams.IsPhysicsEnabled;
        IsPhysicsGravityEnabled = physicsParams.IsPhysicsGravityEnabled;
        Mass = physicsParams.Mass;
        GravityAdditionalStrength = physicsParams.GravityAdditionalStrength;
        PhysicsMaterial = physicsParams.PhysicsMaterial;
        RigidbodyConstraints = physicsParams.RigidbodyConstraints;
        PhysicsEcludeLayers = physicsParams.PhysicsEcludeLayers;
    }
}


