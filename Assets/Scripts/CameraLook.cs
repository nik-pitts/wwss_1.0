using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    private float XMove;
    private float YMove;
    private float XRotation;
    
    [SerializeField] private Transform PlayerBody;
    private Vector2 cameraInput; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;         
    }

    void Update()
    {
        if (Context.Instance.GetCurrentCtrl == CtrlMechanism.vrcontroller) {
            return;
        }

        float Sensitivity = Context.Instance.GetCameraSensitivity;

        if (Sensitivity <= 0f) return;

        /*Switch
        switch (Context.Instance.GetCurrentCtrl) {
            case CtrlMechanism.gamepad:
                break;
            case CtrlMechanism.keyboard:                
                break;
            case CtrlMechanism.vrcontroller:
                break;
            default:
                break;
        }
        */

        XMove = cameraInput.x * Sensitivity * Time.deltaTime;
        YMove = cameraInput.y * Sensitivity * Time.deltaTime;

        XRotation -= YMove;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(XRotation, 0, 0); // Rotate Camera
        PlayerBody.Rotate(Vector3.up * XMove); // Rotate Player horizontally
        
    }

    public void SetCameraInput(Vector2 input)
    {
        switch (Context.Instance.GetCurrentCtrl) {
            case CtrlMechanism.gamepad:
                cameraInput = input;
                break;
            case CtrlMechanism.keyboard:
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                cameraInput = mouseDelta;                
                break;
            case CtrlMechanism.vrcontroller:
                cameraInput = input;
                break;
            default:
                break;
        }
        
    }
}
