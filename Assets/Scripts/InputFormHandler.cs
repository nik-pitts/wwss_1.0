using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class InputFormHandler : MonoBehaviour
{
    private TMP_InputField inputField;
    private string filePath;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        filePath = Path.Combine(Application.dataPath, "SavedText.txt");
    }
    
    public void SaveText()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            File.WriteAllText(filePath, inputField.text);
            Debug.Log($"Text saved to {filePath}");
        }
        else
        {
            Debug.LogWarning("Input field is empty, nothing to save!");
        }
    }
}
