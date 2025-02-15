using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour
{
    public Button myButton;

    void Start()
    {
        myButton.onClick.AddListener(() => Debug.Log("Button Clicked!"));
    }
}
