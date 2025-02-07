using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenUrl : MonoBehaviour
{
    public void OpenURL()
    {
        string url = "https://docs.google.com/forms/d/1yUtomq9JwJDQdd3plJMIkoDF9jhyBx0xKwfhaf_qnlU/prefill";
        Application.OpenURL(url);
        Debug.Log("Start with existing journal template.");
    }

    public void MakeOwnForm()
    {
        string url = "https://docs.google.com/forms/";
        Application.OpenURL(url);
        Debug.Log("Make your own template using google form.");
    }
}
