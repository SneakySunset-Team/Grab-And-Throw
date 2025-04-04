using UnityEngine;
using UnityEngine.Events;

public class CONFETTI : MonoBehaviour
{
    public UnityEvent _onEnter;

    public string _tag;

    public ParticleSystem _particleSystem;
    private Collider _collider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (_tag != "" & _tag == collision.transform.parent.tag)
        {
            if (_onEnter != null)
            {
                _onEnter.Invoke();
            }
            _particleSystem.Play();
        }
    }
}
