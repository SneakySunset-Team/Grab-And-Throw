using UnityEngine;

public class SOPlayerAnimationManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator _animator;
    private IMovement _movementComponent;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _movementComponent = GetComponent<IMovement>();
    }

    private void Update()
    {
        if (_animator != null)
        {
            float inputDirection = _movementComponent.InputMovementDirection.magnitude;

            //Debug.Log(inputDirection);

            if (inputDirection > 0.1 && _movementComponent.CurrentMovementState == EMovementState.Grounded || _movementComponent.CurrentMovementState == EMovementState.Aiming) { _animator.SetBool("IsRunning", true); }
            else { _animator.SetBool("IsRunning", false); }
        }
    }

}
