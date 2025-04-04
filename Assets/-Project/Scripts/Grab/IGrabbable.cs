using System;
using UnityEngine;

public enum EGrabbingState { Passif, Grabbed, Thrown, Stunned, Snapped }

public interface IGrabbable
{
    public Action<IGrabber> OnGrabbedAction { get; set; }
    public Action<IGrabber> OnReleasedAction { get; set; }

    public EGrabbingState CurrentState { get; set; }

    public void OnGrabbed(IGrabber grabber, Transform grabberTransformParent , IGrabbable grabberGrabbable);

    public void OnThrowed(Vector3 force, IGrabbable grabberGrabbable);

    public void OnReleased();

    public void SetFocused(bool value);

    public void ChangeState(EGrabbingState newState);
    public void Stun(float stunDuration);

    public T GetGrabbableComponent<T>();
}
