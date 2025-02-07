using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FormManager : MonoBehaviour
{
    [SerializeField] TMP_InputField reponse;
    private string preFilledUrl;
    private string formResponseUrl;

    void Start()
    {
        preFilledUrl = CopyPasteHandler.LoadPreFilledURL();
        formResponseUrl = ConvertToFormResponseURL(preFilledUrl);
        Debug.Log(formResponseUrl);
        //private string formResponseUrl = "https://docs.google.com/forms/d/e/1FAIpQLSdnMwTtLy5Y6KOZMV4xPtN9r4Tzn2cd60K1mhXrHPhWqDLj2w/formResponse";
    }

    public void Send(string entryId)
    {
        // Send() is called when user hits enter
        StartCoroutine(Post(reponse.text, entryId));
    }

    IEnumerator Post(string responseText, string entryId)
    {
        WWWForm form = new WWWForm();
        form.AddField(entryId, responseText);

        UnityWebRequest www = UnityWebRequest.Post(formResponseUrl, form);

        yield return www.SendWebRequest();
    }

    public string ConvertToFormResponseURL(string preFilledUrl)
    {
        // Find the position of '?' to split parameters
        int queryIndex = preFilledUrl.IndexOf('?');
        if (queryIndex == -1)
        {
            throw new ArgumentException("Invalid Google Form URL: No query parameters found.");
        }

        // Extract base URL and query parameters
        string baseUrl = preFilledUrl.Substring(0, queryIndex);
        // Replace "viewform" with "formResponse" in the base URL
        string formResponseUrl = baseUrl.Replace("/viewform", "/formResponse");

        return formResponseUrl;
    }
}
