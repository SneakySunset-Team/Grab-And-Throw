using System;
using UnityEngine;

[Flags]
public enum ECustomPhysicsConstraints
{
    None = 0,
    PositionX = 1,
    PositionY = 2,
    PositionZ = 4,
    RotationX = 8,
    RotationY = 16,
    RotationZ = 32,
    AllPosition = PositionX | PositionY | PositionZ,
    AllRotation = RotationX | RotationY | RotationZ,
    All = AllPosition | AllRotation
}

public static class GTRigidbodyExtensions
{
    public static RigidbodyConstraints SetFlaggedConstraints(this Rigidbody rb, ECustomPhysicsConstraints constraints)
    {
        RigidbodyConstraints unityConstraints = RigidbodyConstraints.None;

        if (constraints.HasFlag(ECustomPhysicsConstraints.PositionX))
            unityConstraints |= RigidbodyConstraints.FreezePositionX;

        if (constraints.HasFlag(ECustomPhysicsConstraints.PositionY))
            unityConstraints |= RigidbodyConstraints.FreezePositionY;

        if (constraints.HasFlag(ECustomPhysicsConstraints.PositionZ))
            unityConstraints |= RigidbodyConstraints.FreezePositionZ;

        if (constraints.HasFlag(ECustomPhysicsConstraints.RotationX))
            unityConstraints |= RigidbodyConstraints.FreezeRotationX;

        if (constraints.HasFlag(ECustomPhysicsConstraints.RotationY))
            unityConstraints |= RigidbodyConstraints.FreezeRotationY;

        if (constraints.HasFlag(ECustomPhysicsConstraints.RotationZ))
            unityConstraints |= RigidbodyConstraints.FreezeRotationZ;

        return unityConstraints;
    }
}