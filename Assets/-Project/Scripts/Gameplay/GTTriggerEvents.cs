using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class GTTriggerEvents : MonoBehaviour
{
    public GameObject _triggeringObject;

    public UnityEvent _onTriggerEnter;
    public UnityEvent _onTriggerExit;

    private GameObject _objectInside;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.gameObject == _triggeringObject)
        {
            _objectInside = other.gameObject;
            _onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject == _triggeringObject)
        {
            _objectInside = null;
            _onTriggerExit?.Invoke();
        }
    }

    public void InstanciateOnTrigger(GameObject _objectToInstantiate)
    {
        GameObject.Instantiate(_objectToInstantiate, _objectInside.transform.position, _objectInside.transform.rotation);
    }

    public void DisableTriggeringObject()
    {
        _triggeringObject.SetActive(false);
    }
}
