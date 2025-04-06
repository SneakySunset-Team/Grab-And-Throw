using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static GTCharacter_Move;

public enum EMovementState { Grounded, AirBorn, Disabled, Aiming, Grabbed };
[RequireComponent(typeof(Rigidbody)/*, typeof(CharacterController)*/)]
public class GTCharacter_Move : SerializedMonoBehaviour, IMovement
{
    // ********** PUBLIC **********************************

    public Action<EMovementState> OnStateEnterEvent { get; set; }
    public Action<EMovementState> OnStateExitEvent { get; set; }

    public Vector2 InputMovementDirection { get ; set; }

    public float rotationSpeed;

    public void OnJump()
    {
        GTGrabDetection parent = transform.root.GetComponent<GTGrabDetection>();
        if (parent != null && parent != GetComponent<GTGrabDetection>())
        {
            parent.ReleasePlayer();
            _rigidbody?.AddForce(_movementDictionary.MovementParamsDictionary[EMovementState.Grounded].JumpStrength * Vector3.up, ForceMode.VelocityChange);
        }
        else
            _rigidbody?.AddForce(_movementDictionary.MovementParamsDictionary[_currentMovementState].JumpStrength * Vector3.up, ForceMode.VelocityChange);
    }

    public bool IsGrounded()
    {
        return _isGrounded /*_characterController.isGrounded*/;
    }

    public void ChangeState(EMovementState newState)
    {
        OnStateExitEvent?.Invoke(_currentMovementState);
        OnStateEnterEvent?.Invoke(newState);
        _currentMovementState = newState;
    }

    public void SetStateToCached()
    {
        if (_cachedMovementState == _currentMovementState) return;
        OnStateExitEvent?.Invoke(_currentMovementState);
        OnStateEnterEvent?.Invoke(_cachedMovementState);
        _currentMovementState = _cachedMovementState;
    }

    public void CacheState()
    {
        _cachedMovementState = _currentMovementState;
    }


    // ********** UNITY ***********************************

    [SerializeField]
    private GTSO_MovementDictionary _movementDictionary;

    [SerializeField, PropertySpace(20)]
    private EMovementState _currentMovementState = EMovementState.Grounded;

    [SerializeField]
    private float _groundAngleLimit;

    [SerializeField]
    private LayerMask _groundLayerMask;

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

        OnMovementStateEnter(_currentMovementState);

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


        if (_currentMovementState == EMovementState.Aiming)
        {
            ApplyRotation(new Vector3(InputMovementDirection.x, 0, InputMovementDirection.y));  
            ResetMovement();    
            ComputeGravity();
            _currentVelocity.y = _yVelocityCounter;
            ApplyMovement();
        }
        else if(_currentMovementState == EMovementState.Grabbed)
        {
            ComputeMovement();
            ApplyRotation(_currentVelocity);
        }
        else if (_currentMovementState != EMovementState.Disabled && _currentMovementState != EMovementState.Aiming)
        {
            ComputeGravity();
            ComputeMovement();
            ApplyMovement();
            ApplyRotation(_currentVelocity);
        }

    }

    private void Update()
    {
        bool isGrounded = IsGrounded();
        if(_currentMovementState == EMovementState.AirBorn && isGrounded)
        {
            ChangeState(EMovementState.Grounded);
        }
        else if(_currentMovementState == EMovementState.Grounded && !isGrounded)
        {
            ChangeState(EMovementState.AirBorn);
        }
    }


    // ********** RESTRICTED ******************************

    /*  CharacterController _characterController;*/
    private Camera _camera;
    private Rigidbody _rigidbody;
    GTGrabbableObject _grabbableComponent;
    private float _currentAcceleration;
    private Vector3 _currentVelocity;
    private Vector2 _currentMovementDirection;
    private EMovementState _cachedMovementState;
    private Transform _platformTransform;
    private Vector3 _platformPreviousPosition;
    [ShowInInspector]
    bool _isGrounded;
    Rigidbody _platformRb;
    float _yVelocityCounter;

    private List<Collider> _grounds = new List<Collider>();

    private void ComputeGravity()
    {
        if(_currentMovementState == EMovementState.AirBorn )
        {
           _yVelocityCounter += Physics.gravity.y * _movementDictionary.MovementParamsDictionary[_currentMovementState].GravityMultiplier * Time.fixedDeltaTime;
           //_currentVelocity.y = _yVelocityCounter;
        }
        else if(_currentMovementState == EMovementState.Grounded)
        {
            _yVelocityCounter = _rigidbody.linearVelocity.y;
            //_currentVelocity.y = -1;
        }
        else if(_currentMovementState == EMovementState.Aiming)
        {
            if (_isGrounded)
            {
                _yVelocityCounter = _rigidbody.linearVelocity.y;
            }
            else 
            {
                _yVelocityCounter += Physics.gravity.y * _movementDictionary.MovementParamsDictionary[_currentMovementState].GravityMultiplier * Time.fixedDeltaTime;
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
            _movementDictionary.MovementParamsDictionary[_currentMovementState].Deceleration :
            _movementDictionary.MovementParamsDictionary[_currentMovementState].Acceleration;

        _currentAcceleration = Mathf.Clamp(_currentAcceleration + accelerationMult / accelerationMagnitude * Time.fixedDeltaTime, 0, 1);

        _currentVelocity = _currentAcceleration * 
            new Vector3(_currentMovementDirection.x, 0, _currentMovementDirection.y) *
            _movementDictionary.MovementParamsDictionary[_currentMovementState].Speed * 
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
                //_characterController.enabled = true;
                break;
            case EMovementState.AirBorn:
                _yVelocityCounter = _rigidbody.linearVelocity.y;
                //_characterController.enabled = true;
                break;
            case EMovementState.Disabled:
                //_characterController.enabled = false;
                break;
            case EMovementState.Aiming:
                _yVelocityCounter = _rigidbody.linearVelocity.y;
                break;
        }

    }

    private void OnMovementStateTick(float deltaTime)
    {
        if (_platformTransform != null)
        {
            Vector3 delta = _platformTransform.position - _platformPreviousPosition;
            delta.y = delta.magnitude > 1 ? delta.y : 0;
            _rigidbody.MovePosition(transform.position + delta);
            transform.position += delta;
        }
        switch (_currentMovementState)
        {
            case EMovementState.Grounded:
                
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, transform.localScale.y * 2 + 2f, _groundLayerMask))
                {
/*                    if (hit.transform.root.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Debug.Log(rb.linearVelocity);
                        _platformPreviousPosition = rb.linearVelocity;
                    }*/
                    _platformTransform = hit.transform;
                    _platformPreviousPosition = hit.transform.position;
                }
                else
                {
                    _platformTransform = null;
                }
                break;
            case EMovementState.AirBorn:
                _platformTransform = null;
                break;
            case EMovementState.Disabled:
                break;
            case EMovementState.Aiming:
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, transform.localScale.y * 2 + 2f, _groundLayerMask))
                {
                    /*                    if (hit.transform.root.TryGetComponent<Rigidbody>(out var rb))
                                        {
                                            Debug.Log(rb.linearVelocity);
                                            _platformPreviousPosition = rb.linearVelocity;
                                        }*/
                    _platformTransform = hit2.transform;
                    _platformPreviousPosition = hit2.transform.position;
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
        switch(_currentMovementState)
        {
            case EMovementState.Grounded:
                break;
            case EMovementState.AirBorn:
                break;
            case EMovementState.Disabled:
                break;
            case EMovementState.Aiming:
                break;
        }
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
            if (gtObj.CurrentState == EGrabbingState.Thrown)
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
}


public interface IMovement
{
    public Action<EMovementState> OnStateEnterEvent { get; set; }
    public Action<EMovementState> OnStateExitEvent { get; set; }

    public Vector2 InputMovementDirection { get; set; }

    public void OnJump(); 
}
