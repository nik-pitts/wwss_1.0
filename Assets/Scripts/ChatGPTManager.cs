using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using Unity.VisualScripting;
using UnityEngine.Events;
using Random=UnityEngine.Random;

public class ChatGPTManager : MonoBehaviour
{
    public OnResponseEvent OnResponse;
    [SerializeField] private WebScrapper ws;
    [SerializeField] private FormManager fm;
    
    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string>
    {
        
    }

    // This part should be replaced to attacehd API key and organzation code
    // first place param : OpenAIApi 
    // second place param : organizaiton key
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    private List<int> randomQuery;
    private int formQNum;
    private string PROMPT = "You are a friendly boy dog and your name is Shooting Star. " +
                            "You are a great listener, listen carefully to people's talk " +
                            "and deeply empathize with their emotions. " + 
                            "You are a friend or an emotional supporter." + 
                            "Become a doggy friend with who you are talking to!" +
                            "Rather than to repeat user's response you should come up with different " +
                            "choice of vocabularies and expressions." +
                            "Do not include emojis in your response.";

    public void Start()
    {
        // Random question query for today
        int numQ = ws.formQuestions.Count;
        randomQuery = genRandNums(0, numQ);
        
        // Add initial prompt to the messages list
        ChatMessage promptMessage = new ChatMessage();
        promptMessage.Role = "system";
        promptMessage.Content = PROMPT; 
        messages.Add(promptMessage);
        
        AskChatGPT("Give warm greetings!");
    }

    public async void AskChatGPT(string newText)
    {
        // record response to the form
        // when recording condition is met
        if (messages.Count > 5 && randomQuery.Count > 0)
        {
            fm.Send(ws.entryIds[formQNum]);
            randomQuery.RemoveAt(0);
        }
            
        // add user input
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";
        messages.Add(newMessage);
        
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";

        var response = await openAI.CreateChatCompletion(request);
        if (response.Choices != null && response.Choices.Count > 0) 
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);
            // chatResponse = response from Shooting Star

            if (messages.Count > 3 && randomQuery.Count > 0)
            {
                formQNum = randomQuery[0];
                AddFormQWithResponse(chatResponse, randomQuery[0]);
            }
            else
            {
                OnResponse.Invoke(chatResponse.Content);
            }
        }
    }

    public async void AddFormQWithResponse(ChatMessage chatResponse, int qNum)
    {
        String instruction = "Instruction: Combine #1 marked response with #2 marked question naturally," +
                             "and formulate one complete response." +
                             "You must rewrite the whole response to combine two.";
        String formerResponse = chatResponse.Content;
        String formQToAdd = ws.formQuestions[qNum];
        ChatMessage askAgain = new ChatMessage();
        askAgain.Content = instruction + "#1." + formerResponse + "#2." + formQToAdd;
        askAgain.Role = "user";
        messages.Add(askAgain);
        
        // make a request to ChatGPT
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";
        var response = await openAI.CreateChatCompletion(request);
        if (response.Choices != null && response.Choices.Count > 0) 
        {
            var formulatedReponse = response.Choices[0].Message;
            messages.Add(formulatedReponse);
            OnResponse.Invoke(formulatedReponse.Content);
        }
    }

    private List<int> genRandNums(int from, int to)
    {
        List<int> numbers = new List<int>();
        for (int i = from; i < to; i++)
        {
            numbers.Add(i);
        }
        for (int i = 0; i < numbers.Count; i++)
        {
            int j = Random.Range(0, numbers.Count - 1);
            int element = numbers[j];
            numbers[j] = numbers[i];
            numbers[i] = element;
        }

        for (int i = 0; i < numbers.Count; i++)
        {
            Debug.Log(numbers[i]);
        }
    
        return numbers;
    }
}
