using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class GT_BombObject : GTGrabbableObject
{
    [SerializeField] private float _explosionDelay = 3f;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _explosionPower = 5f;
    [SerializeField] private VisualEffect _visualEffectExplosionPrefab;
    private Vector3 _startingPosition;

    protected override void Awake()
    {
        base.Awake();
        _startingPosition = transform.position; 
        Init();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void OnGrabbed(IGrabber grabber, Transform grabbableTransformParent,  IGrabbable grabbableGrabber)
    {
        base.OnGrabbed(grabber, grabbableTransformParent, grabbableGrabber);
        StartCoroutine(ExploseAndRespawn());
    }

    private IEnumerator ExploseAndRespawn()
    {
        yield return new WaitForSeconds(_explosionDelay);

        Instantiate(_visualEffectExplosionPrefab, transform.position, Quaternion.identity);
        Explode();
        Init();
    }

    private void Explode()
    {
        List<IGrabber> grabbers = new List<IGrabber>();
        List<IGrabbable> grabbables = new List<IGrabbable>();
        foreach (var body in GetOverlappingColliders())
        {

            IGrabber grabber = body.gameObject.transform.root.GetComponent<IGrabber>();
            IGrabbable grabbable = body.gameObject.GetComponentInParent<IGrabbable>();

            if (grabber != null)
                grabbers.Add(grabber);

            if (grabbable == null)
                continue;

            if (!grabbables.Contains(grabbable))
                grabbables.Add(grabbable);
        }

        foreach(IGrabber grabber in grabbers)
        {
            grabber.Reset();
        }

        foreach (IGrabbable grabbableObject in grabbables)
        {
            Rigidbody rb = grabbableObject.GetGrabbableComponent<Rigidbody>();
            if (rb != null) // should never be null but we still check
            {
                Vector3 direction = (grabbableObject.GetGrabbableComponent<Transform>().position - this.transform.position);
                /*               direction.y = 0;
                               direction = direction.normalized;

                               if (direction.x == 0 && direction.z == 0)
                                   direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                */

                grabbableObject.Stun(2f); 
                //rb.AddForce(_explosionPower * direction, ForceMode.Impulse);
                //rb.AddExplosionForce(_explosionPower, transform.localPosition, _explosionRadius, 0, ForceMode.Impulse);
                rb.AddForceAtPosition(direction * _explosionPower, transform.position, ForceMode.Impulse);
            }
        }
    }

    private void Init()
    {
        ChangeState(EGrabbingState.Passif);
        this.transform.position = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10)); // TODO : set bounds
        _rb.linearVelocity = Vector3.zero;
        transform.position = _startingPosition;
    }

    public Collider[] GetOverlappingColliders()
    {
        // Obtenir la position et le rayon de la sphère en tenant compte de l'échelle
        Vector3 center = transform.position;
        float radius = _explosionRadius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        // Récupérer tous les colliders qui se chevauchent
        Collider[] overlappingColliders = Physics.OverlapSphere(center, radius);
        _explosionPosition = transform.position;
        StartCoroutine(DrawDebugExplosionSphere());
        return overlappingColliders;
    }

    bool _isSphereDrawn;
    Vector3 _explosionPosition;
    IEnumerator DrawDebugExplosionSphere()
    {
        _isSphereDrawn = true;
        yield return new WaitForSeconds(2);
        _isSphereDrawn = false;
    }
    void OnDrawGizmos()
    {
        if (_isSphereDrawn)
        {
            Gizmos.DrawWireSphere(_explosionPosition, _explosionRadius); 
        }
    }

}
