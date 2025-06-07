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
    public static GamepadControl Instance { get; private set; }

    private PlayerInput input;
    private CameraLook cameraLook;
    
    
    public Vector2 currentMovement;
    public bool movementPressed;
    public float recordingOnProgress;
    public bool isRunning = false;
    public float moveSpeed = 3.0f;
    public float runSpeedMultiplier = 2.0f;
    public Vector2 cameraRotation;
    public float rotationSpeed = 1.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;
    private ShootingStarMove shootingStar;

    private Vector3 velocity = Vector3.zero;
    private CharacterController characterController;
    
    [SerializeField] private GameObject throwablePrefab; // Assign in Unity Inspector
    [SerializeField] private Transform throwPoint; // Assign in Unity Inspector
    [SerializeField] private float throwForce = 15f;
    private GameObject heldObject;
    private bool isAiming = false;
    private Vector3 aimDirection;

    private void Awake()
    {
        // init
        if (Instance == null) {
            Instance = this;
        } else {            
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        input = new PlayerInput();

        InputSystem.onDeviceChange += (device, change) => {
            Debug.Log($"Device: {device.displayName}, Layout: {device.layout}, Change: {change}");
        };

        switch (Context.Instance.GetCurrentCtrl) {
            case CtrlMechanism.gamepad:
                RegisterControllerInput();
                break;
            case CtrlMechanism.keyboard:
                RegisterKeyboardInput();
                break;
            case CtrlMechanism.vrcontroller:
                RegisterVRInput();
                break;
            default:
                break;
        }
        
    }

    void RegisterControllerInput()
    {
        input.CharacterControlsController.Disable();
        input.CharacterControls.Enable();
        input.CharacterControlsKeyboard.Disable();

        // move function
        input.CharacterControls.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.sqrMagnitude > 0;
        };
        input.CharacterControls.Movement.canceled += ctx => {
            currentMovement = Vector2.zero;
            movementPressed = false;
        };

        // run function
        input.CharacterControls.Run.performed += ctx => isRunning = true;
        input.CharacterControls.Run.canceled += ctx => isRunning = false;

        // record function
        input.CharacterControls.Record.performed += ctx => recordingOnProgress = ctx.ReadValue<float>();

        // throw function
        input.CharacterControls.Throw.performed += ctx => StartAiming();
        input.CharacterControls.Throw.canceled += ctx => ThrowObject();

        // camera rotation function
        input.CharacterControls.Rotation.performed += ctx => {
            cameraRotation = ctx.ReadValue<Vector2>();
            if (cameraLook != null) {
                cameraLook.SetCameraInput(cameraRotation);
            }
        };
        input.CharacterControls.Rotation.canceled += ctx => {
            cameraRotation = Vector2.zero;
            if (cameraLook != null)
                cameraLook.SetCameraInput(Vector2.zero);
        };

        // show mouse cursor
        InputSystem.onAnyButtonPress.CallOnce(ctrl => {
            if (ctrl.device is Mouse) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            } else if (ctrl.device is Gamepad) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        });
    }

    /// <summary>
    /// register for keyboard control
    /// </summary>
    void RegisterKeyboardInput()
    {
        input.CharacterControlsController.Disable();
        input.CharacterControls.Disable();
        input.CharacterControlsKeyboard.Enable();

        // move function
        input.CharacterControlsKeyboard.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.sqrMagnitude > 0;
        };
        input.CharacterControlsKeyboard.Movement.canceled += ctx => {
            currentMovement = Vector2.zero;
            movementPressed = false;
        };

        // run function
        input.CharacterControlsKeyboard.Run.performed += ctx => isRunning = true;
        input.CharacterControlsKeyboard.Run.canceled += ctx => isRunning = false;

        // record function
        input.CharacterControlsKeyboard.Record.performed += ctx => recordingOnProgress = ctx.ReadValue<float>();

        // throw function
        input.CharacterControlsKeyboard.Throw.performed += ctx => StartAiming();
        input.CharacterControlsKeyboard.Throw.canceled += ctx => ThrowObject();

        // camera rotation function
        input.CharacterControlsKeyboard.Rotation.performed += ctx => {
            cameraRotation = ctx.ReadValue<Vector2>();
            if (cameraLook != null) {
                cameraLook.SetCameraInput(cameraRotation);
            }
        };
        input.CharacterControlsKeyboard.Rotation.canceled += ctx => {
            cameraRotation = Vector2.zero;
            if (cameraLook != null)
                cameraLook.SetCameraInput(Vector2.zero);
        };

        // show mouse cursor
        input.CharacterControlsKeyboard.ShowMouse.performed += ctx => {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        };
    }

    void RegisterVRInput()
    {        
        input.CharacterControlsController.Enable();
        input.CharacterControls.Disable();
        input.CharacterControlsKeyboard.Disable();

        Debug.Log("VR input enabled");

        // move function
        input.CharacterControlsController.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.sqrMagnitude > 0;
        };
        input.CharacterControlsController.Movement.canceled += ctx => {
            currentMovement = Vector2.zero;
            movementPressed = false;
        };

        // run function
        input.CharacterControlsController.Run.performed += ctx => isRunning = true;
        input.CharacterControlsController.Run.canceled += ctx => isRunning = false;

        // record function
        input.CharacterControlsController.Record.performed += ctx => recordingOnProgress = ctx.ReadValue<float>();

        // throw function
        input.CharacterControlsController.Throw.performed += ctx => StartAiming();
        input.CharacterControlsController.Throw.canceled += ctx => ThrowObject();

        // camera rotation function
        input.CharacterControlsController.Rotation.performed += ctx => {
            cameraRotation = ctx.ReadValue<Vector2>();
            if (cameraLook != null) {
                cameraLook.SetCameraInput(cameraRotation);
            }
        };
        input.CharacterControlsController.Rotation.canceled += ctx => {
            cameraRotation = Vector2.zero;
            if (cameraLook != null)
                cameraLook.SetCameraInput(Vector2.zero);
        };
    }

    void Start()
    {
        // choose cameraLook
        cameraLook = 
            (Context.Instance.GetCurrentCtrl == CtrlMechanism.gamepad || Context.Instance.GetCurrentCtrl == CtrlMechanism.keyboard) 
        ? Context.Instance.GetDesktopCameraObject.GetComponent<CameraLook>()
        : Context.Instance.GetQuestCameraObject.GetComponent<CameraLook>();

        characterController = GetComponent<CharacterController>();
        shootingStar = FindObjectOfType<ShootingStarMove>();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        if (isAiming)
        {
            HandleAiming();
            UpdateBallPosition();
        }
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
        float currentSpeed = isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed;

        if (movementPressed)
        {
            velocity = Vector3.Lerp(velocity, moveDirection * currentSpeed, Time.deltaTime * acceleration);
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
            Vector3 moveDirection = (cameraLook.transform.forward * currentMovement.y +
                                     cameraLook.transform.right * currentMovement.x);
            moveDirection.y = 0;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
    
    private void StartAiming()
    {
        Debug.Log("Aiming...");
        if (heldObject != null) return;

        isAiming = true;
        shootingStar.StartAiming();

        // Instantiate object in hand
        heldObject = Instantiate(throwablePrefab, throwPoint.position, throwPoint.rotation);
        heldObject.transform.SetParent(throwPoint);
        heldObject.transform.localPosition = Vector3.zero;
        
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // Prevent physics movement while aiming
        }
    }
    
    private void HandleAiming()
    {
        if (isAiming)
        {
            aimDirection = cameraLook.transform.forward;
            heldObject.transform.rotation = Quaternion.LookRotation(aimDirection);
        }
    }

    private void UpdateBallPosition()
    {
        heldObject.transform.position = throwPoint.position;
        heldObject.transform.rotation = throwPoint.rotation;
    }
    
    private void ThrowObject()
    {
        Debug.Log("Throwing...");
        if (heldObject == null) return;

        isAiming = false;
        shootingStar.StopAiming(); 

        heldObject.transform.SetParent(null);
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(aimDirection * throwForce, ForceMode.Impulse);
        }

        shootingStar.ChaseBall(heldObject);
        heldObject = null; // Reset reference
    }
    
    // private void OnEnable() { input.CharacterControls.Enable(); }
    // private void OnDisable() { input.CharacterControls.Disable(); }
}