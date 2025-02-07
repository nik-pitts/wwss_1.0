using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
public class DateTimeManager : MonoBehaviour
{
    private const string API_URL = "https://worldtimeapi.org/api/ip";
    DateTime currentDateTime = DateTime.Now;

    private void Start()
    {
        StartCoroutine(GetRealDateTimeFromAPI());
        Debug.Log(currentDateTime);
    }

    public DateTime GetStartDateTime()
    {
        return currentDateTime;
    }

    public DateTime GetDateTimeNow()
    {
        StartCoroutine(GetRealDateTimeFromAPI());
        return currentDateTime;
    }
    
    IEnumerator GetRealDateTimeFromAPI()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(API_URL);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + webRequest.error);
        }
        else
        {
            string timeDate = webRequest.downloadHandler.text;
            currentDateTime = ParseDateTIme(timeDate);
        }
    }

    DateTime ParseDateTIme(string dateTime)
    {
        string date = Regex.Match(dateTime, @"^\d{4}-\d{2}-\d{2}$").Value;
        string time = Regex.Match(dateTime, @"^\d{2}:\d{2}:\d{2}$").Value;
        return DateTime.Parse(String.Format("{0} {1}", date, time));
    }
}
