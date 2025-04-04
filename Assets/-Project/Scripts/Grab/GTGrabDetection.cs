using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GTGrabDetection : MonoBehaviour, IGrabber
{

    // ****** PUBLIC      ******************************************
    public IGrabbable GetGrabbedObject()
    {
        return _myGrabbable;
    }

    public void Grab()
    {
        //Start Throw
        if (_objectGrabbed != null)
        {
            if (_throwEnum != null)
            {
                StopCoroutine(_throwEnum);
            }
            _throwEnum = ThrowCoroutine();
            StartCoroutine(_throwEnum);

            return;
        }

        //No Close Grabbable
        if (_nearestGrabbableObject == null) return;
        else if (_nearestGrabbableObject != null && _nearestGrabbableObject.CurrentState == EGrabbingState.Snapped
             || _nearestGrabbableObject.CurrentState == EGrabbingState.Grabbed)
        {
            _grabbableObjectsInRange.Remove(_nearestGrabbableObject);
            _nearestGrabbableObject = null;
            return;
        }

        _grabbableObjectsInRange.Remove(_nearestGrabbableObject);
        _objectGrabbed = _nearestGrabbableObject;
        _objectGrabbed.SetFocused(false);
        FMODUnity.RuntimeManager.PlayOneShot($"event:/Chara/{GTPlayerManager.Instance.GetPlayerFmodName(_tag.GetPlayerTag())}/Grab");
        _nearestGrabbableObject = null;
        _objectGrabbed.OnGrabbed(this, _grabbedObjectTransform, _myGrabbable);

        _rig.weight = 1;
        var grabPoints = _objectGrabbed.GetGrabbableComponent<GTGrabbableObject>().GetGrabPoints();
        _rigConstraintLeft.data.target = grabPoints.Item1;
        _rigConstraintRight.data.target = grabPoints.Item2;
        GetComponentInChildren<RigBuilder>().Build();

        _isGrabbingInputComplete = false;
    }

    public void ReleasePlayer()
    {
        if (_objectGrabbed == null)
            return;

        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        Vector3 force = Vector3.up;
        _objectGrabbed.OnReleased();
        _objectGrabbed = null;
        Reset();
    }


    public void Release()
    {
        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        _objectGrabbed = null;
        Reset();
    }

    public void Attack()
    {
       /* //No Close Grabbable
        if (_nearestGrabbableObject == null || _nearestGrabbableObject.CurrentState == EGrabbingState.Snapped) return;

        Vector3 direction = (_nearestGrabbableObject.GetGrabbableComponent<Transform>().position - this.transform.position);

        _nearestGrabbableObject.Stun(1f);
        _nearestGrabbableObject.GetGrabbableComponent<Rigidbody>().AddForceAtPosition(direction * 5, transform.position, ForceMode.Impulse);*/
    }

    public void Reset()
    {
        if (_objectGrabbed != null)
        {
            _objectGrabbed.OnThrowed(Vector3.zero, _myGrabbable);
            _objectGrabbed = null;
        }

        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;
        _chargingThrowTime = 0;
        _throwPower = _gtsoThrowSettings.MaxThrowPower;
        _rig.weight = 0;

        _rigConstraintLeft.data.target = null;
        _rigConstraintRight.data.target = null;
        GetComponentInChildren<RigBuilder>().Build();
    }

    public void OnThrowInput()
    {
        // If Holding nothing return
        if (_objectGrabbed == null)
            return;

        // If Is same input as grabbing something return
        if (!_isGrabbingInputComplete)
        {
            _isGrabbingInputComplete = true;
            return;
        }

        // If is throw needs a charge time stop charge time
        _isChargingThrow = false;
        return;
    }

    public IGrabbable GetNearestGrabbableObject()
    {
        IGrabbable closestObject = null;

        if (_grabbableObjectsInRange.Count > 0)
        {
            float minDistance = float.PositiveInfinity;

            foreach (IGrabbable obj in _grabbableObjectsInRange)
            {
                float distance = (transform.position - obj.GetGrabbableComponent<Transform>().position).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObject = obj;
                }
            }
        }

        return closestObject;
    }

    public bool IsGrabbing()
    {
        return _objectGrabbed != null;
    }

    public T GetGrabberComponent<T>()
    {
        return GetComponent<T>();
    }


    // ****** UNITY      ******************************************

    [SerializeField] private BoxCollider _collider;
    [SerializeField] private Transform _grabbedObjectTransform;
    [SerializeField] private GTSO_ThrowSettings _gtsoThrowSettings;
    [SerializeField] private Transform _previewTransform;
    [SerializeField] private float _previewMaxZScale;
    [SerializeField] private ChainIKConstraint _rigConstraintLeft;
    [SerializeField] private ChainIKConstraint _rigConstraintRight;
    [SerializeField] private float _detectionRange;
    [SerializeField] private LayerMask _detectionLayer;


    private void Start()
    {
        _throwPower = _gtsoThrowSettings.MaxThrowPower;
        _myGrabbable = GetGrabberComponent<IGrabbable>();
        if (_myGrabbable != null)
        {
            _myGrabbable.OnGrabbedAction += OnGrabbed;
        }
        _rig = GetComponentInChildren<Rig>();
        _tag = GetComponent<GTPlayerTag>();
    }

    private void OnDisable()
    {
        if (_myGrabbable != null)
        {
            _myGrabbable.OnGrabbedAction -= OnGrabbed;
        }
    }

    private void FixedUpdate()
    {
        if (Physics.SphereCast(transform.position + transform.up * .2f, .3f, transform.forward, out RaycastHit hitInfo, _detectionRange, _detectionLayer, QueryTriggerInteraction.Ignore))
        {
            var grabbable = hitInfo.transform.GetComponent<IGrabbable>();
            if (grabbable != _nearestGrabbableObject)
            {
                if (_nearestGrabbableObject != null)
                    _nearestGrabbableObject.SetFocused(false);

                if (grabbable != null && grabbable.CurrentState != EGrabbingState.Snapped && grabbable.CurrentState != EGrabbingState.Grabbed)
                    _nearestGrabbableObject = grabbable;
                else
                    _nearestGrabbableObject = null;
            }

            if (_nearestGrabbableObject != null
                && _nearestGrabbableObject.GetGrabbableComponent<GTGrabbableObject>().ComponentID != EComponentType.Player)
                _nearestGrabbableObject.SetFocused(true);
        }
        else if (_nearestGrabbableObject != null)
        {
            _nearestGrabbableObject.SetFocused(false);
            _nearestGrabbableObject = null;
        }
    }

    // ****** RESTRICTED      ******************************************

    private List<IGrabbable> _grabbableObjectsInRange = new List<IGrabbable>();
    private IGrabbable _objectGrabbed;
    private IGrabbable _nearestGrabbableObject;
    private IGrabbable _myGrabbable;
    private bool _isGrabbingInputComplete;
    private bool _isChargingThrow;
    private float _chargingThrowTime;
    private float _throwPower;
    private IEnumerator _throwEnum;
    private Rig _rig;
    private GTPlayerTag _tag;
    private ConfigurableJoint _joint;
    private int _lineResolution;
    private LineRenderer _lineRenderer;

    private void ApplyThrow()
    {
        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        float angleInRadians = _gtsoThrowSettings.ThrowAngle * Mathf.Deg2Rad;
        Vector3 horizontalDir = transform.forward;

        _throwPower = Mathf.Clamp(_throwPower, _gtsoThrowSettings.MinThrowPower, _gtsoThrowSettings.MaxThrowPower);

        float horizontalForce = _throwPower * Mathf.Cos(angleInRadians);
        float verticalForce = _throwPower * Mathf.Sin(angleInRadians);

        Vector3 force = (horizontalDir * horizontalForce) + (Vector3.up * verticalForce);

        FMODUnity.RuntimeManager.PlayOneShot($"event:/Chara/{GTPlayerManager.Instance.GetPlayerFmodName(_tag.GetPlayerTag())}/Throw");

        _objectGrabbed.OnThrowed(force, _myGrabbable);

        _objectGrabbed = null;
        Reset();
    }

    private void OnGrabbed(IGrabber grabber)
    {
        IGrabbable grabbable = grabber.GetGrabberComponent<IGrabbable>();
        for(int i = _grabbableObjectsInRange.Count - 1; i >= 0; i--)
        {
            if (_grabbableObjectsInRange[i] == grabbable)
            {
                _grabbableObjectsInRange[i].SetFocused(false);
                if (_nearestGrabbableObject == _grabbableObjectsInRange[i])
                    _nearestGrabbableObject = null;
                _grabbableObjectsInRange.RemoveAt(i);
            }
        }
    }

    private IEnumerator ThrowCoroutine()
    {
        _isChargingThrow = true;
        _chargingThrowTime = 0;

        var characterMove = GetGrabberComponent<GTCharacter_Move>();

        if (_gtsoThrowSettings.IsMovementDisabled)
        {
            characterMove.CacheState();
            characterMove.ChangeState(EMovementState.Aiming);
        }

        while (_isChargingThrow)
        {
            _chargingThrowTime += Time.deltaTime;

            switch (_gtsoThrowSettings.ThrowType)
            {
                case ThrowType.PressToThrow:
                    _previewTransform.localScale = new Vector3(1, .1f, _previewMaxZScale);
                    break;
                case ThrowType.ChargeXTimeToThrow:
                    _previewTransform.localScale = new Vector3(1, .1f, _previewMaxZScale * (_chargingThrowTime / _gtsoThrowSettings.MaxThrowChargeTime));
                    break;
                case ThrowType.ChargeUntilMaxValueToThrow:
                    _chargingThrowTime = Mathf.Clamp(_chargingThrowTime, 0f, _gtsoThrowSettings.MaxThrowChargeTime);
                    _throwPower = _gtsoThrowSettings.MaxThrowPower * (_chargingThrowTime / _gtsoThrowSettings.MaxThrowChargeTime);
                    _previewTransform.localScale = new Vector3(1, .1f, _previewMaxZScale * (_chargingThrowTime / _gtsoThrowSettings.MaxThrowChargeTime));
                    break;
                case ThrowType.ChargeFreeToThrow:
                    _throwPower = _gtsoThrowSettings.MaxThrowPower * Mathf.PingPong(_chargingThrowTime, _gtsoThrowSettings.MaxThrowChargeTime);
                    _previewTransform.localScale = new Vector3(1, .1f, _previewMaxZScale * Mathf.PingPong(_chargingThrowTime, _gtsoThrowSettings.MaxThrowChargeTime));
                    break;
            }

            // If throw has a maximum charge time Stop charging and throw
            if (_gtsoThrowSettings.ThrowType == ThrowType.ChargeXTimeToThrow && _chargingThrowTime >= _gtsoThrowSettings.MaxThrowChargeTime)
            {
                _isChargingThrow = false;
            }
            yield return null;
        }

        characterMove.SetStateToCached();

        ApplyThrow();
        
        _throwEnum = null;
    }
    
    private void GetPreviewPoints()
    {
        float angleInRadians = _gtsoThrowSettings.ThrowAngle * Mathf.Deg2Rad;
        Vector3 horizontalDir = transform.forward;

        _throwPower = Mathf.Clamp(_throwPower, _gtsoThrowSettings.MinThrowPower, _gtsoThrowSettings.MaxThrowPower);

        float horizontalForce = _throwPower * Mathf.Cos(angleInRadians);
        float verticalForce = _throwPower * Mathf.Sin(angleInRadians);

        Vector3 force = (horizontalDir * horizontalForce) + (Vector3.up * verticalForce);

        Vector3 velocity = (force / _objectGrabbed.GetGrabbableComponent<Rigidbody>().mass) * Time.fixedDeltaTime;
        float flightDuration = (velocity.y * 2) / Physics.gravity.y;

        float stepTime = flightDuration / _lineResolution;

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < _lineResolution; i++)
        {
            float stepTimePassed = stepTime * i;
            Vector3 MovementVector = new Vector3(
                velocity.x * stepTimePassed,
                velocity.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                velocity.z * stepTimePassed);

            RaycastHit hit;
/*            if(Physics.Raycast(_objectGrabbed.GetGrabbableComponent<Transform>().position, - MovementVector, out hit, MovementVector.magnitude))
            {
                break;
            }*/
            points.Add(-MovementVector + _objectGrabbed.GetGrabbableComponent<Transform>().position);
        }
        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }
}
