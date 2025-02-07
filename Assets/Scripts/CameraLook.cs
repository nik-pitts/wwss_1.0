using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    private float XMove;
    private float YMove;
    private float XRotation;
    
    [SerializeField] private Transform PlayerBody;
    public float Sensitivity = 40f;
    private Vector2 cameraInput; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        XMove = cameraInput.x * Sensitivity * Time.deltaTime;
        YMove = cameraInput.y * Sensitivity * Time.deltaTime;

        XRotation -= YMove;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(XRotation, 0, 0); // Rotate Camera
        PlayerBody.Rotate(Vector3.up * XMove); // Rotate Player horizontally
    }

    public void SetCameraInput(Vector2 input)
    {
        cameraInput = input;
    }
}
