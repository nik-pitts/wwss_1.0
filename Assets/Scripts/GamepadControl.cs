using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class GamepadControl : MonoBehaviour
{
    private PlayerInput input;
    private CameraLook cameraLook;
    
    public Vector2 currentMovement;
    public bool movementPressed;
    public float recordingOnProgress;
    public float moveSpeed = 3.0f;
    public Vector2 cameraRotation;
    public float rotationSpeed = 1.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;

    private Vector3 velocity = Vector3.zero;
    private CharacterController characterController;
    
    void Awake()
    {
        input = new PlayerInput();
        input.CharacterControls.Movement.performed += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.sqrMagnitude > 0;
        };
        input.CharacterControls.Movement.canceled += ctx => 
        {
            currentMovement = Vector2.zero;
            movementPressed = false;
        };
        
        input.CharacterControls.Record.performed += ctx => recordingOnProgress = ctx.ReadValue<float>();
        
        input.CharacterControls.Rotation.performed += ctx =>
        {
            cameraRotation = ctx.ReadValue<Vector2>();
            if (cameraLook != null)
            {
                cameraLook.SetCameraInput(cameraRotation);
            }
        };
        input.CharacterControls.Rotation.canceled += ctx => cameraRotation = Vector2.zero;
        
        InputSystem.onAnyButtonPress.CallOnce(ctrl =>
        {
            if (ctrl.device is Mouse)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (ctrl.device is Gamepad)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        });
    }

    void Start()
    {
        cameraLook = FindObjectOfType<CameraLook>();
        characterController = GetComponent<CharacterController>();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 cameraForward = cameraLook.transform.forward;
        Vector3 cameraRight = cameraLook.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * currentMovement.y + cameraRight * currentMovement.x).normalized;

        if (movementPressed)
        {
            velocity = Vector3.Lerp(velocity, moveDirection * moveSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * deceleration);
        }

        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        RaycastHit hit;

        if (Physics.Raycast(newPosition + Vector3.up * 1.5f, Vector3.down, out hit, 2.0f, LayerMask.GetMask("Terrain")))
        {
            newPosition.y = hit.point.y;
        }

        characterController.Move(newPosition - transform.position);
    }

    void HandleRotation()
    {
        if (movementPressed)
        {
            Vector3 moveDirection = (cameraLook.transform.forward * currentMovement.y + cameraLook.transform.right * currentMovement.x);
            moveDirection.y = 0;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void OnEnable() { input.CharacterControls.Enable(); }
    private void OnDisable() { input.CharacterControls.Disable(); }
}
