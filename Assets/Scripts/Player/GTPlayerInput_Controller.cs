using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GTPlayerInput_Controller : MonoBehaviour
{
    private IMovement _movementComponent;
    private IGrabber _grabberComponent;
    private Camera _camera;
    private void Start()
    {
        _camera = Camera.main;
        _movementComponent = GetComponent<IMovement>();
        _grabberComponent = GetComponent<IGrabber>();
    }
    // ****** PUBLIC      ******************************************


    public void OnJump(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _movementComponent?.OnJump();
        }
    }

    public void OnMove(InputValue inputValue)
    {
        if (_movementComponent != null)
        {

            _camera = Camera.main;
            Vector2 moveInput = inputValue.Get<Vector2>();
            Vector3 cameraForward = _camera.transform.forward;
            Vector3 cameraRight = _camera.transform.right;

            // Project vectors to ground plane (zero out Y component for pure horizontal movement)
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize(); // Normalize after zeroing Y
            cameraRight.Normalize();

            // Create movement direction using the horizontal components
            Vector3 movementDirection = moveInput.x * cameraRight + moveInput.y * cameraForward;

            _movementComponent.InputMovementDirection = movementDirection;
        }
    }

    public void OnMove(InputAction.CallbackContext cbx)
    {
        if (_movementComponent != null)
        {
            _camera = FindFirstObjectByType<Camera>();
            Vector2 moveInput = cbx.ReadValue<Vector2>();
            Vector2 cameraForward = new Vector2(_camera.transform.forward.x, _camera.transform.forward.z);
            Vector2 cameraRight = new Vector2(_camera.transform.right.x, _camera.transform.right.z);
            // Project vectors to ground plane (zero out Y component for pure horizontal movement)
            cameraForward.Normalize(); // Normalize after zeroing Y
            cameraRight.Normalize();

            // Create movement direction using the horizontal components
            Vector3 movementDirection = moveInput.x * cameraRight + moveInput.y * cameraForward;

            _movementComponent.InputMovementDirection = movementDirection;
        }
    }

    public void OnGrabThrow(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _grabberComponent?.Grab();
        }
        else if (cbx.canceled)
        {
            _grabberComponent?.OnThrowInput();
        }
    }

    public void OnAttack(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _grabberComponent?.Attack();
        }
    }


    // ****** RESTRICTED      ******************************************

    private IMovement _movementComponent;
    private IGrabber _grabberComponent;

    private void Start()
    {
        _movementComponent = GetComponent<IMovement>();
        _grabberComponent = GetComponent<IGrabber>();
    }

   
}
