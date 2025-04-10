using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

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
        _objectGrabbed.OnGrabbed(this, _grabbedItemParent, _myGrabbable);

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
        if(_objectGrabbed == null) return;

        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        _objectGrabbed.OnReleased();
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

    public void SwitchTarget()
    {
        _targetIndex = (_targetIndex + 1) % _maxNumOfDetectionInRadius;
    }
    
    public void RotateHeldItem()
    {
        if (_objectGrabbed != null && _objectGrabbed.GetGrabbableComponent<GTPlayerTag>() == null)
        {
            var joint = _objectGrabbed.GetGrabbableComponent<ConfigurableJoint>();
            joint.angularYMotion = ConfigurableJointMotion.Free;
            var rb = _objectGrabbed.GetGrabbableComponent<Rigidbody>();
            Quaternion rotation = rb.rotation * Quaternion.Euler(0, 90, 0);
            rb.MoveRotation(rotation);
            StartCoroutine(RelockJointCoroutine(joint));
        }
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

    [SerializeField] private GTSO_ThrowSettings _gtsoThrowSettings;
    [SerializeField] private Transform _grabbedItemParent;

    [SerializeField, BoxGroup("Preview")] private Transform _previewTransform;
    [SerializeField, BoxGroup("Preview")] private float _previewMaxZScale;
    [SerializeField, BoxGroup("Preview"), Range(3, 30)] private int _lineResolution;

    [SerializeField, BoxGroup("Rigging")] private ChainIKConstraint _rigConstraintLeft;
    [SerializeField, BoxGroup("Rigging")] private ChainIKConstraint _rigConstraintRight;
 
    [SerializeField, BoxGroup("Detection")] private float _detectionRange;
    [SerializeField, BoxGroup("Detection")] private float _detectionRadius;
    [SerializeField, BoxGroup("Detection")] private LayerMask _detectionLayer;
    [SerializeField, BoxGroup("Detection")] private Vector3 _detectionCenterOffset;
    [SerializeField, BoxGroup("Detection")] private bool _debugDetectionSphere;

    [SerializeField, BoxGroup("Detection"), Range(1, 10)] private int _maxNumOfDetectionInRadius;
    [SerializeField, BoxGroup("Detection"), ReadOnly] private int _targetIndex;
    [SerializeField, BoxGroup("Detection"), ReadOnly] private List<IGrabbable> _detectedGrabbables;
    

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _lineRenderer = GetComponent<LineRenderer>();
        _throwPower = _gtsoThrowSettings.MaxThrowPower;
        _myGrabbable = GetGrabberComponent<IGrabbable>();
        if (_myGrabbable != null)
        {
            _myGrabbable.OnGrabbedAction += OnGrabbed;
        }
        _rig = GetComponentInChildren<Rig>();
        _tag = GetComponent<GTPlayerTag>();
        _rigidbody = GetComponent<Rigidbody>();
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
        if (_isChargingThrow)
        {
            UpdateThrowPreview();
        }
        else
        {
            _lineRenderer.positionCount = 0;
        }
        
        if(_objectGrabbed != null)
        {
            return;
        }

        int hitCount = Physics.SphereCastNonAlloc(
            transform.position + _detectionCenterOffset,
            _detectionRadius,
            transform.forward,
            _raycastHitCache,
            _detectionRange,
            _detectionLayer,
            QueryTriggerInteraction.Ignore);



        if (hitCount != 0)
        {
            _grabbableObjectsInRange.Clear();
            _grabbableObjectsInRange.Capacity = hitCount;
            for (int i = 0; i < Mathf.Min(hitCount, _maxNumOfDetectionInRadius); i++)
            {
                IGrabbable temp = _raycastHitCache[i].transform.GetComponent<IGrabbable>();
                if (temp != null)
                {
                    if (temp == _myGrabbable)
                    {
                        continue;
                    }

                    _grabbableObjectsInRange.Add(temp);
                }
            }
        }
        
        if(_grabbableObjectsInRange.Count() != 0)
        {
            IGrabbable grabbable = _grabbableObjectsInRange[_targetIndex % _grabbableObjectsInRange.Count];
            
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

    private void OnDrawGizmos()
    {
        if (_debugDetectionSphere)
        {
            Gizmos.DrawWireCube(transform.position + _detectionCenterOffset + transform.forward * _detectionRadius / 2, 
                transform.forward * _detectionRange + transform.right * _detectionRadius + transform.up * _detectionRadius);
            Gizmos.DrawWireSphere(transform.position + _detectionCenterOffset + transform.forward * _detectionRange, _detectionRadius);
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
    private Rigidbody _rigidbody;
    private ConfigurableJoint _joint;
    private LineRenderer _lineRenderer;
    private PlayerInput _playerInput;
    private RaycastHit[] _raycastHitCache = new RaycastHit[10];


    private void ApplyThrow()
    {
        _previewTransform.localScale = Vector3.zero;
        _isChargingThrow = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        Vector3 horizontalDir;
        if (_playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.LogError("Mouse raycast hit nothing");
                return;
            }
            Vector3 worldPos = hitInfo.point;
            horizontalDir = (worldPos - transform.position).normalized;
        }
        else
        {
            horizontalDir = transform.forward;
        }

        float angleInRadians = _gtsoThrowSettings.ThrowAngle * Mathf.Deg2Rad;

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
                    break;
                case ThrowType.ChargeXTimeToThrow:
                    break;
                case ThrowType.ChargeUntilMaxValueToThrow:
                    _chargingThrowTime = Mathf.Clamp(_chargingThrowTime, 0f, _gtsoThrowSettings.MaxThrowChargeTime);
                    _throwPower = _gtsoThrowSettings.MaxThrowPower * (_chargingThrowTime / _gtsoThrowSettings.MaxThrowChargeTime);
                    break;
                case ThrowType.ChargeFreeToThrow:
                    float currentPingpongValue = Mathf.PingPong(
                        _chargingThrowTime, 
                        _gtsoThrowSettings.MaxThrowChargeTime - _gtsoThrowSettings.MinThrowPower / _gtsoThrowSettings.MaxThrowPower
                        ) / _gtsoThrowSettings.MaxThrowChargeTime;
                    
                    _throwPower = _gtsoThrowSettings.MaxThrowPower * (  _gtsoThrowSettings.MinThrowPower / _gtsoThrowSettings.MaxThrowPower + currentPingpongValue);
    
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

    private void UpdateThrowPreview()
    {
        float angleInRadians = _gtsoThrowSettings.ThrowAngle * Mathf.Deg2Rad;
        
        Vector3 horizontalDir;
        if (_playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.LogError("Mouse raycast hit nothing");
                return;
            }
            Vector3 worldPos = hitInfo.point;
            horizontalDir = (worldPos - transform.position).normalized;
        }
        else
        {
            horizontalDir = transform.forward;
        }
        
        float horizontalForce = _throwPower * Mathf.Cos(angleInRadians);
        float verticalForce = _throwPower * Mathf.Sin(angleInRadians);
        Vector3 force = (horizontalDir * horizontalForce) + (Vector3.up * verticalForce);
        
        // Get the grabbable component and its parameters
        GTGrabbableObject grabbableObject = _objectGrabbed.GetGrabbableComponent<GTGrabbableObject>();
        
        GTPhysicsParams physicsParams = grabbableObject.GetPhysicParams(EGrabbingState.Thrown);
        
        float objectMass = physicsParams.Mass;
        
        // Check if gravity is enabled for this object
        bool isGravityEnabled = !physicsParams.IsPhysicsGravityEnabled;
        
        // Get the constant force component (if it exists)
        Vector3 constantForceVector = Vector3.up * physicsParams.GravityAdditionalStrength;
        Vector3 constantAcceleration = constantForceVector / objectMass;
        
        // Initial velocity from throw
        Vector3 velocity = force / objectMass;
        
        // Add gravity to the constant acceleration only if gravity is enabled
        Vector3 totalAcceleration = constantAcceleration;
        if (isGravityEnabled)
        {
            totalAcceleration += Physics.gravity;
        }
        
        // Estimate flight duration based on whether gravity is enabled
        float flightDuration;
        if (isGravityEnabled)
        {
            flightDuration = EstimateFlightDuration(velocity, totalAcceleration);
        }
        else
        {
            // For objects without gravity, use a fixed time or distance-based estimation
            // since they won't fall back down
            flightDuration = EstimateNonGravityFlightDuration(velocity);
        }
        
        float stepTime = flightDuration / _lineResolution;
        
        List<Vector3> points = new List<Vector3>();
        Vector3 startPos = _objectGrabbed.GetGrabbableComponent<Transform>().position;
        points.Add(startPos); // Add starting position
        
        for (int i = 1; i < _lineResolution; i++)
        {
            float stepTimePassed = stepTime * i;
            
            // Calculate position using the equation of motion with constant acceleration:
            // position = initialPosition + initialVelocity * time + 0.5 * acceleration * time^2
            Vector3 newPos = startPos + 
                            velocity * stepTimePassed + 
                            0.5f * totalAcceleration * stepTimePassed * stepTimePassed;
            
            // Cast ray from previous point to new point
            Vector3 direction = newPos - points[i - 1];
            float distance = direction.magnitude;
            RaycastHit[] hits = Physics.RaycastAll(points[i - 1], direction.normalized, distance);
            
            Debug.DrawRay(points[i - 1], direction, Color.red);
            
            bool isHit = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.rigidbody != _rigidbody && hit.rigidbody != _objectGrabbed.GetGrabbableComponent<Rigidbody>())
                {
                    // If hit something other than our object, mark the hit point and break
                    points.Add(hit.point);
                    isHit = true;
                    break;
                }
            }
            
            if (isHit) break;
            points.Add(newPos);
        }
        
        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }
    
    // Helper method to estimate flight duration with constant acceleration
    private float EstimateFlightDuration(Vector3 initialVelocity, Vector3 acceleration)
    {
        // This is a heuristic - for a more accurate duration, you'd need to solve
        // for when the trajectory intersects with the ground or other objects
        float timeToReachPeak = Mathf.Abs(initialVelocity.y / acceleration.y);
        return timeToReachPeak * 2.5f; // Multiply by a factor to ensure we see beyond the peak
    }
    
    // Helper method for objects without gravity
    private float EstimateNonGravityFlightDuration(Vector3 initialVelocity)
    {
        // For objects without gravity, we can estimate based on distance or a fixed time
        // Here we use velocity magnitude to determine a reasonable flight time to show
        float baseDistance = 20f; // Adjust based on your game's scale
        float speedMagnitude = initialVelocity.magnitude;
        
        if (speedMagnitude < 0.001f)
            return 3f; // Default duration if velocity is nearly zero
            
        return baseDistance / speedMagnitude;
    }
    
    IEnumerator RelockJointCoroutine(ConfigurableJoint joint)
    {
        yield return new WaitForFixedUpdate();
        
        _objectGrabbed.ReinitializeJoint();
    }

}
