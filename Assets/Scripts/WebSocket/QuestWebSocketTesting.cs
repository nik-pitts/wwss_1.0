using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestWebSocketTesting : MonoBehaviour
{
    [Header("Optional callbacks")]
    public UnityEvent onAPressed;
    public UnityEvent onBPressed;

    void Update()
    {
        // --- A button ------------------------------------------------------
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) {
            Debug.Log("A button pressed");
            onAPressed?.Invoke();
        }

        // --- B button ------------------------------------------------------
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) {
            Debug.Log("B button pressed");
            onBPressed?.Invoke();
        }
    }
}
