using UnityEngine;

public interface IGrabber
{
    public void Grab();
    public void Attack();
    public void Reset();

    public void Release();

    public bool IsGrabbing();

    public void OnThrowInput();

    public IGrabbable GetGrabbedObject();

    public  T GetGrabberComponent<T>();
}
