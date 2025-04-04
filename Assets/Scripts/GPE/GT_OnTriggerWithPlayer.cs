using UnityEngine;
using UnityEngine.Events;

public class GT_OnTriggerWithPlayer : MonoBehaviour
{
    public UnityEvent _onCollisionEnterEvent;
    public UnityEvent _onCollisionExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.TryGetComponent<GTPlayerTag>(out GTPlayerTag tag))
        {
            _onCollisionEnterEvent?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.TryGetComponent<GTPlayerTag>(out GTPlayerTag tag))
        {
            _onCollisionExitEvent?.Invoke();
        }
    }

    public void SetPlayerStartIndex(int index)
    {
        Debug.Log($"Set player index to {index}");

        GTPlayerManager.Instance.SetPlayerStartIndex(index);
    }
}
