using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class GamepadControl : MonoBehaviour
{
    private PlayerInput input;
    private CameraLook cameraLook;
    
    public Vector2 currentMovement;
    public bool movementPressed;
    public float recordingOnProgress;
    public float moveSpeed = 3.0f;
    public Vector2 cameraRotation;   // Right Stick Rotation
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
    }


    // Start is called before the first frame update
    void Start()
    {
        cameraLook = FindObjectOfType<CameraLook>();
        characterController = GetComponent<CharacterController>(); // ✅ Use Unity's Character Controller for better physics
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // ✅ Get the camera's forward direction
        Vector3 cameraForward = cameraLook.transform.forward;
        Vector3 cameraRight = cameraLook.transform.right;
        
        // ✅ Remove any vertical tilt from the camera (only use XZ plane)
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // ✅ Convert Left Stick input into world-space movement relative to the camera
        Vector3 moveDirection = (cameraForward * currentMovement.y + cameraRight * currentMovement.x).normalized;

        // ✅ Apply acceleration and deceleration
        if (movementPressed)
        {
            velocity = Vector3.Lerp(velocity, moveDirection * moveSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * deceleration);
        }

        // ✅ Apply movement
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (movementPressed)
        {
            // ✅ Smoothly rotate towards movement direction
            Vector3 moveDirection = (cameraLook.transform.forward * currentMovement.y + cameraLook.transform.right * currentMovement.x);
            moveDirection.y = 0; // Keep rotation on XZ plane
            
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
