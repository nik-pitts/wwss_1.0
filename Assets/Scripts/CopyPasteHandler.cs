using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class CopyPasteHandler
{
    static string filePath = Path.Combine(Application.dataPath, "SavedText.txt");
    static TMP_InputField inputField;

    public static string PasteFromClipBoard()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.multiline = true;
        textEditor.Paste();
        Debug.Log(textEditor.text);
        return textEditor.text;
    }
    
    public static string LoadPreFilledURL()
    {
        if (File.Exists(filePath))
        {
            string preFilledUrl = File.ReadAllText(filePath);
            Debug.Log("Text loaded successfully.");
            return preFilledUrl;
        }
        else
        {
            Debug.LogWarning("No saved text found.");
            return null;
        }
    }
}
