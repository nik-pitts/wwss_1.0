using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using UnityEngine.Rendering.Universal;

public class WebScrapper : MonoBehaviour
{
    public Dictionary<int, string> formQuestions = new Dictionary<int, string>();
    public Dictionary<int, string> entryIds = new Dictionary<int, string>();
    
    // Start is called before the first frame update
    void Awake()
    {
        // Send get request
        String url = CopyPasteHandler.LoadPreFilledURL();
        // this is test form url
        //String url = "https://docs.google.com/forms/d/e/1FAIpQLSdnMwTtLy5Y6KOZMV4xPtN9r4Tzn2cd60K1mhXrHPhWqDLj2w/viewform?entry.787779239=8&entry.1851785395=The+weather+was+so+great.+I+ate+healthy+breakfast,+lunch+and+dinner.&entry.170349516=It+was+very+time+consuming+to+add+desired+features+for+my+game.&entry.2121724733=No.&entry.456285609=Yes!+I+had+short+chat+with+Kim.&entry.502436794=I%27m+thankful+that+I+can+delve+into+the+project+that+I+want+to+do.";
        var httpClient = new HttpClient();
        var html = httpClient.GetStringAsync(url).Result;
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        // Get questions from the form
        var questions = htmlDocument.DocumentNode.SelectNodes("//span[@class='M7eMe']");
        for (int i = 0; i < questions.Count; i++)
        {
            formQuestions.Add(i, questions[i].InnerText);
        }   
        
        // Get entry number from the pre-filled url
        var splitResult = url.Split("&");
        for (int i = 1; i < splitResult.Length; i++)
        {
            var secondSplit = splitResult[i].Split("=");
            entryIds.Add(i-1, secondSplit[0]);
        }

        for (int i = 0; i < entryIds.Count; i++)
        {
            Debug.Log("Entry Id : " + entryIds[i] + ", Form Question : " + formQuestions[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
