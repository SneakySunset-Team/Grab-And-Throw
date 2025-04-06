using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;


[RequireComponent(typeof(Collider))]
public class GTTriggerEvents : MonoBehaviour
{
    [SerializeField] private bool _useTag;

    [SerializeField] private GameObject _triggeringObject;
    [SerializeField] private string _triggeringTag;

    [SerializeField] private UnityEvent _onTriggerEnter;
    [SerializeField] private UnityEvent _onTriggerExit;

    private GameObject _objectInside;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.gameObject == _triggeringObject  || other.transform.root.tag == _triggeringTag)
        {
            _objectInside = other.gameObject;
            _onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject == _objectInside)
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

    public void ForceStartAnimation()
    {
        Animation _animation = transform.root.GetComponent<Animation>();

        _animation.Play();
    }
}
