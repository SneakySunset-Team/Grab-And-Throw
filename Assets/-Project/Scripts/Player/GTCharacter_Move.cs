using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.VFX;
using static GTCharacter_Move;

public enum EMovementState { Grounded, AirBorn, Disabled, Aiming, Grabbed};
[RequireComponent(typeof(Rigidbody)/*, typeof(CharacterController)*/)]
public class GTCharacter_Move : SerializedMonoBehaviour, IMovement
{
    // ********** PUBLIC **********************************

    public Action<EMovementState> OnStateEnterEvent { get; set; }
    public Action<EMovementState> OnStateExitEvent { get; set; }

    public Vector2 InputMovementDirection { get; set; }

    public float rotationSpeed;

    public void OnJump()
    {
        if (CurrentMovementState != EMovementState.Grounded && CurrentMovementState != EMovementState.Aiming)
        {
            if (_jumpCacheInEnum != null)
            {
                StopCoroutine(_jumpCacheInEnum);
            }
            _jumpCacheInEnum = JumpCacheCoroutine();
            StartCoroutine(_jumpCacheInEnum);
            return;
        }

        ApplyJump();

    }

    public bool IsGrounded()
    {
        return _isGrounded /*_characterController.isGrounded*/;
    }

    public void ChangeState(EMovementState newState)
    {
        OnStateExitEvent?.Invoke(CurrentMovementState);
        CurrentMovementState = newState;
        OnStateEnterEvent?.Invoke(newState);
    }

    public void SetStateToCached()
    {
        if (_cachedMovementState == CurrentMovementState) return;
        OnStateExitEvent?.Invoke(CurrentMovementState);
        OnStateEnterEvent?.Invoke(_cachedMovementState);
        CurrentMovementState = _cachedMovementState;
    }

    public void CacheState()
    {
        _cachedMovementState = CurrentMovementState;
    }


    // ********** UNITY ***********************************

    [SerializeField]
    private GTSO_MovementDictionary _movementDictionary;

    [SerializeField, PropertySpace(20)]
    public EMovementState CurrentMovementState { get; private set; }

    [SerializeField]
    private float _groundAngleLimit;

    [SerializeField]
    public LayerMask _groundLayerMask;

    [SerializeField]
    private float _jumpCacheInDuration;

    [SerializeField]
    private VisualEffect _moveVisualEffect;

    [SerializeField]
    private VisualEffect _landVisualEffect;


    private IEnumerator Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _grabbableComponent = GetComponent<GTGrabbableObject>();
        if(_grabbableComponent != null)
        {
            _grabbableComponent.OnStateEnterEvent += OnGrabbableStateChangeEnter;
        }

        OnStateEnterEvent += OnMovementStateEnter;
        OnStateExitEvent += OnMovementStateExit;

        _pointTest = new GameObject().transform;

        OnMovementStateEnter(CurrentMovementState);
        yield return new WaitUntil(()=> Camera.main != null);
        _camera = Camera.main;
    }

    private void OnDisable()
    {
        if(_grabbableComponent != null)
        {
            _grabbableComponent.OnStateEnterEvent -= OnGrabbableStateChangeEnter;
        }

        OnStateEnterEvent -= OnMovementStateEnter;
        OnStateExitEvent -= OnMovementStateExit;
    }

    private void FixedUpdate()
    {
        OnMovementStateTick(Time.fixedDeltaTime);


        if(_isGrounded && !_previousGrounded)
        {
            _landVisualEffect.Play();
        }
        _previousGrounded = _isGrounded;

        if (CurrentMovementState == EMovementState.Aiming)
        {
            ApplyRotation(new Vector3(InputMovementDirection.x, 0, InputMovementDirection.y));  
            ResetMovement();    
            ComputeGravity();
            _currentVelocity.y = _yVelocityCounter;
            ApplyMovement();
        }
        else if(CurrentMovementState == EMovementState.Grabbed)
        {
            ComputeMovement();
            ApplyRotation(_currentVelocity);
        }
        else if (CurrentMovementState != EMovementState.Disabled)
        {
            ComputeGravity();
            ComputeMovement();
            ApplyMovement();
            ApplyRotation(_currentVelocity);
            if(_previousHorizontalVelocity == Vector3.zero && _currentVelocity != Vector3.zero)
            {
                _moveVisualEffect.Play();
            }
            else if (_previousHorizontalVelocity != Vector3.zero && _currentVelocity == Vector3.zero)
            {
                _moveVisualEffect.Stop();
            }
        }
        _previousHorizontalVelocity = _currentVelocity;
    }

    private void Update()
    {
        bool isGrounded = IsGrounded();
        if(CurrentMovementState == EMovementState.AirBorn && isGrounded)
        {
            ChangeState(EMovementState.Grounded);
        }
        else if(CurrentMovementState == EMovementState.Grounded && !isGrounded)
        {
            ChangeState(EMovementState.AirBorn);
        }
    }


    // ********** RESTRICTED ******************************

    private Camera _camera;
    private Rigidbody _rigidbody;
    GTGrabbableObject _grabbableComponent;
    private float _currentAcceleration;
    private Vector3 _currentVelocity;
    private Vector2 _currentMovementDirection;
    private EMovementState _cachedMovementState;
    [ShowInInspector]
    private bool _isGrounded;
    private bool _previousGrounded;
    private float _yVelocityCounter;
    private Vector3 _previousHorizontalVelocity;
    private IEnumerator _jumpCacheInEnum;
    private List<Collider> _grounds = new List<Collider>();

    private Transform _platformTransform;
    private Vector3 _platformPreviousPosition;
    private Transform _pointTest;

    private void ComputeGravity()
    {
        if(CurrentMovementState == EMovementState.AirBorn )
        {
           _yVelocityCounter += Physics.gravity.y * _movementDictionary.MovementParamsDictionary[CurrentMovementState].GravityMultiplier * Time.fixedDeltaTime;
           //_currentVelocity.y = _yVelocityCounter;
        }
        else if(CurrentMovementState == EMovementState.Grounded)
        {
            _yVelocityCounter = _rigidbody.linearVelocity.y;
            //_currentVelocity.y = -1;
        }
        else if(CurrentMovementState == EMovementState.Aiming)
        {
            if (_isGrounded)
            {
                _yVelocityCounter = _rigidbody.linearVelocity.y;
            }
            else 
            {
                _yVelocityCounter += Physics.gravity.y * _movementDictionary.MovementParamsDictionary[CurrentMovementState].GravityMultiplier * Time.fixedDeltaTime;
            }
        }
        else
        {
            _yVelocityCounter = 0;
        }
    }

    private void ComputeMovement()
    {
        bool isDecelerating = InputMovementDirection == Vector2.zero;

        if (!isDecelerating)
        {
            _currentMovementDirection = InputMovementDirection;
        }

        float accelerationMult = isDecelerating ?  -1 : 1;
        float accelerationMagnitude = isDecelerating ?
            _movementDictionary.MovementParamsDictionary[CurrentMovementState].Deceleration :
            _movementDictionary.MovementParamsDictionary[CurrentMovementState].Acceleration;

        _currentAcceleration = Mathf.Clamp(_currentAcceleration + accelerationMult / accelerationMagnitude * Time.fixedDeltaTime, 0, 1);

        _currentVelocity = _currentAcceleration * 
            new Vector3(_currentMovementDirection.x, 0, _currentMovementDirection.y) *
            _movementDictionary.MovementParamsDictionary[CurrentMovementState].Speed * 
            Time.fixedDeltaTime;

        _currentVelocity.y = _yVelocityCounter;
    }

    private void ResetMovement()
    {
        _currentVelocity = new Vector3(0, 0, 0);
    }

    private void ApplyMovement()
    {
        _rigidbody.linearVelocity = _currentVelocity /*+ _platformPreviousPosition*/;
    }

    private void ApplyRotation(Vector3 direction)
    {
        direction.y = 0;
        if (direction == Vector3.zero) return;
        //transform.forward = direction;
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void OnGrabbableStateChangeEnter(EGrabbingState newState)
    {
        switch (newState)
        {
            case EGrabbingState.Passif:
                transform.rotation = Quaternion.identity;
                ChangeState(EMovementState.Grounded);
                break;
            case EGrabbingState.Grabbed:
                ChangeState(EMovementState.Grabbed);
                break;
            case EGrabbingState.Thrown:
                ChangeState(EMovementState.Disabled);
                break;
            case EGrabbingState.Stunned:
                ChangeState(EMovementState.Disabled);
                break;
        }
    }

    private void OnMovementStateEnter(EMovementState newState)
    {
        switch (newState)
        {
            case EMovementState.Grounded:
                if (_currentVelocity != Vector3.zero)
                {
                    _moveVisualEffect.Play();
                }
                if(_jumpCacheInEnum != null)
                {
                    ApplyJump();
                    _jumpCacheInEnum = null;
                }
                break;
            case EMovementState.AirBorn:
                _yVelocityCounter = _rigidbody.linearVelocity.y;
                break;
            case EMovementState.Disabled:
                break;
            case EMovementState.Aiming:
                _yVelocityCounter = _rigidbody.linearVelocity.y;
                break;
            case EMovementState.Grabbed:
                _rigidbody.linearVelocity = Vector3.zero;
                break;
        }

    }

    private void OnMovementStateTick(float deltaTime)
    {
        if (_platformTransform != null)
        {
            Vector3 delta = _pointTest.position - _platformPreviousPosition;
            //delta.y = delta.magnitude > 1 ? delta.y : 0;
            _rigidbody.MovePosition(transform.position + delta);
            transform.position += delta;
        }

        switch (CurrentMovementState)
        {
            case EMovementState.Grounded:
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, transform.localScale.y * 2 + 2f, _groundLayerMask))
                {
                    _platformTransform = hit.transform;
                    _pointTest.parent = hit.transform;
                    _pointTest.position = hit.point;
                    _platformPreviousPosition = _pointTest.position;
                }
                else
                {
                    _platformTransform = null;
                    _pointTest.parent = null;
                }
                break;
            case EMovementState.AirBorn:
                _platformTransform = null;
                _pointTest.parent = null;
                break;
            case EMovementState.Disabled:
                break;
            case EMovementState.Aiming:
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, transform.localScale.y * 2 + 2f, _groundLayerMask))
                {
                    _platformTransform = hit2.transform;
                    _pointTest.parent = hit2.transform;
                    _pointTest.position = hit2.point;
                    _platformPreviousPosition = _pointTest.position;
                }
                else
                {
                    _platformTransform = null;
                }
                break;
        }
    }

    private void OnMovementStateExit(EMovementState newState)
    {
        switch(CurrentMovementState)
        {
            case EMovementState.Grounded:
                _moveVisualEffect.Stop();
                break;
            case EMovementState.AirBorn:
                break;
            case EMovementState.Disabled:
                break;
            case EMovementState.Aiming:
                break;
        }
    }

    private void ApplyJump()
    {
        GTGrabDetection parent = transform.root.GetComponent<GTGrabDetection>();
        if (parent != null && parent != GetComponent<GTGrabDetection>())
        {
            parent.ReleasePlayer();
            _rigidbody?.AddForce(_movementDictionary.MovementParamsDictionary[EMovementState.Grounded].JumpStrength * Vector3.up, ForceMode.VelocityChange);
        }
        else
            _rigidbody?.AddForce(_movementDictionary.MovementParamsDictionary[CurrentMovementState].JumpStrength * Vector3.up, ForceMode.VelocityChange);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.contacts[0].normal.x > -_groundAngleLimit && collision.contacts[0].normal.x < _groundAngleLimit)
        {
            if (!_grounds.Contains(collision.collider))
            {
                _grounds.Add(collision.collider);
            }
            _isGrounded = true;
        }
        else
        {
            if (_grounds.Contains(collision.collider))
            {
                _grounds.Remove(collision.collider);
                if (_grounds.Count == 0)
                {
                    _isGrounded = false;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GTGrabbableObject gtObj = collision.transform.GetComponent<GTGrabbableObject>();
        if (gtObj != null)
        { 
            if (gtObj.CurrentState == EGrabbingState.Thrown && gtObj.GetComponent<Rigidbody>().linearVelocity.magnitude > 3)
            {
                Vector3 direction = (this.transform.position - gtObj.transform.position);

                _grabbableComponent.Stun(1f);
                GetComponent<Rigidbody>().AddForceAtPosition(direction * 5, transform.position, ForceMode.Impulse);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_grounds.Contains(collision.collider))
        {
            _grounds.Remove(collision.collider);
            if(_grounds.Count == 0)
            {
                _isGrounded = false;
            }
        }
    }

    private IEnumerator JumpCacheCoroutine()
    {
        yield return new WaitForSeconds(_jumpCacheInDuration);
        _jumpCacheInEnum = null;
    }
}


public interface IMovement
{
    public Action<EMovementState> OnStateEnterEvent { get; set; }
    public Action<EMovementState> OnStateExitEvent { get; set; }

    public Vector2 InputMovementDirection { get; set; }

    public EMovementState CurrentMovementState { get;}

    public void OnJump(); 
}
