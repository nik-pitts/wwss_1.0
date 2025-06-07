using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Meta.XR.MultiplayerBlocks.Fusion.Editor;

/// <summary>
/// Global context for managing environment
/// 1. Init all managers
/// 2. Quick references to all main classes
/// 3. Presettings for developing and debugging
/// </summary>
public class Context : MonoBehaviour
{
    public static Context Instance { get; private set; }

    /* 
     * Debugging 
     */
    [Header("For Debugging")]
    [Header("-----------------------------")]
    [Header("Init")]
    [SerializeField]    
    private bool needApiInpput = true;
    public bool GetNeedApiInput => needApiInpput;

    [SerializeField]
    private bool needMemoryInput = true;
    public bool GetNeedMemoryInput => needMemoryInput;

    [SerializeField]
    private bool needAudioInput = true;
    public bool GetNeedAudioInput => needAudioInput;

    [Header("For Game Control")]
    [Header("-----------------------------")]
    [Header("Control")]
    [SerializeField]
    private CtrlMechanism currentCtrlMechanism = CtrlMechanism.gamepad;
    public CtrlMechanism GetCurrentCtrl => currentCtrlMechanism;
    private bool IsGamePad => currentCtrlMechanism == CtrlMechanism.gamepad;
    private bool IsKeyboard => currentCtrlMechanism == CtrlMechanism.keyboard;
    private bool IsVrInput => currentCtrlMechanism == CtrlMechanism.vrcontroller;

    [ShowIf("IsGamePad")]
    [SerializeField]
    private float cameraSensitivityGamepad = 60f;    

    [ShowIf("IsKeyboard")]
    [SerializeField]
    private float cameraSensitivityKeyboard = 100f;

    [ShowIf("IsVrInput")]
    [SerializeField]
    private float cameraSensitivityVR = 100f;


    private float cameraSensitivity;
    [HideInInspector]
    public float GetCameraSensitivity => cameraSensitivity;

    [Header("Monobehaviours")]
    [Header("-----------------------------")]
    [SerializeField]
    private ChatGPTManager gptManager;
    public ChatGPTManager GetGptManager => gptManager;

    [SerializeField]
    private GamepadControl gamepadCtrl;
    public GamepadControl GetGamepadCtrl => gamepadCtrl;

    [Header("Monobehaviours")]
    [Header("-----------------------------")]
    [SerializeField]
    private GameObject desktopCameraObject;
    public GameObject GetDesktopCameraObject => desktopCameraObject;

    [SerializeField]
    private GameObject questCameraObject;
    public GameObject GetQuestCameraObject => questCameraObject;


    private void Awake()
    {
        // Instance
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        // initialize gamepad control
        gamepadCtrl.Init();

        // Initialize GPT Manager
        gptManager.Init();

        // Initialize camera sensitivity
        InitCameraSensitivity();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void InitCameraSensitivity()
    {
        switch (currentCtrlMechanism) {
            case CtrlMechanism.gamepad:
                SwitchDesktopCamera(true);
                cameraSensitivity = cameraSensitivityGamepad;
                break;
            case CtrlMechanism.keyboard:
                SwitchDesktopCamera(true);
                cameraSensitivity = cameraSensitivityKeyboard;
                break;
            case CtrlMechanism.vrcontroller:
                SwitchDesktopCamera(false);
                cameraSensitivity = cameraSensitivityVR;
                break;
            default:
                cameraSensitivity = 0f;
                break;
        }        
    }

    void SwitchDesktopCamera(bool isUsingDesktopCamera)
    {
        desktopCameraObject.SetActive(isUsingDesktopCamera);
        questCameraObject.SetActive(!isUsingDesktopCamera);
    }
}
