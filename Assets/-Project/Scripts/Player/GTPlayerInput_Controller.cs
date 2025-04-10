using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GTPlayerInput_Controller : MonoBehaviour
{

    // ****** PUBLIC      ******************************************

    public void OnJump(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _movementComponent?.OnJump();
        }
    }

    public void OnMove(InputAction.CallbackContext cbx)
    {
        if (_movementComponent != null)
        {
            Vector3 movementDirection;
            Vector2 moveInput = cbx.ReadValue<Vector2>();
            

            Vector2 cameraForward = new Vector2(_camera.transform.forward.x, _camera.transform.forward.z);
            Vector2 cameraRight = new Vector2(_camera.transform.right.x, _camera.transform.right.z);
            
            // Project vectors to ground plane (zero out Y component for pure horizontal movement)
            cameraForward.Normalize(); // Normalize after zeroing Y
            cameraRight.Normalize();

            // Create movement direction using the horizontal components
            movementDirection = moveInput.x * cameraRight + moveInput.y * cameraForward;


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

    public void OnConnectToSurface(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _grabberComponent.ConnectToSurface();
        }
        else if(cbx.canceled)
        {
            _grabberComponent.DisconnectFromSurface();
        }
    }

    public void OnSwitchDetectionTarget_RotateItem(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            if (_grabberComponent.IsGrabbing())
            {
                _grabberComponent.RotateHeldItem();
            }
            else
            {
                _grabberComponent.SwitchTarget();
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            _grabberComponent?.Attack();
        }
    }


    // ****** UNITY     ******************************************


    private IEnumerator Start()
    {
        _movementComponent = GetComponent<IMovement>();
        _grabberComponent = GetComponent<IGrabber>();
        _playerInput = GetComponent<PlayerInput>();
        yield return new WaitUntil(()=> Camera.main != null);
        _camera = Camera.main;
    }

    // ****** RESTRICTED      ******************************************

    private IMovement _movementComponent;
    private IGrabber _grabberComponent;
    private Camera _camera;
    private PlayerInput _playerInput;
}
