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
    private string currentLocation = "Unknown";
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
    private string dynamicPrompt;

    public void Start()
    {
        string timeContext = GetTimeContext();
        
        string initPrompt = $"You are a friendly boy dog named Shooting Star. " +
                               $"You are a great listener, deeply empathetic, and offer emotional support. " +
                               $"You naturally adapt to the current time and provide appropriate conversation. " +
                               $"If the user has not talked to you today, start with a warm greeting based on the time context: {timeContext}. " +
                               $"Do not repeat the user's response. Instead, use varied vocabulary and expressions. " +
                               $"Do not include emojis in your response.";

        // Random question query for today
        int numQ = ws.formQuestions.Count;
        randomQuery = genRandNums(0, numQ);
        
        // Add initial prompt to the messages list
        ChatMessage promptMessage = new ChatMessage{ Role = "system", Content = initPrompt };
        messages.Add(promptMessage);
        
        AskChatGPT("Give warm greetings!");
    }
    public void AskChatGPTFromUI(string newText)
    {
        AskChatGPT(newText, false); // Calls the main method with default behavior
    }

    public async void AskChatGPT(string newText, bool isLocationUpdate = false)
    {
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();

        if (isLocationUpdate)
        {
            // ✅ If triggered by entering a new location, only send the location message (no history)
            request.Messages = new List<ChatMessage>
            {
                new ChatMessage { Content = newText, Role = "user" }
            };
        }
        else
        {
            // ✅ If it's a regular chat message, use the entire conversation history
            request.Messages = messages;
        }

        request.Model = "gpt-4o-mini";

        var response = await openAI.CreateChatCompletion(request);
        if (response.Choices != null && response.Choices.Count > 0) 
        {
            var chatResponse = response.Choices[0].Message;
            Debug.Log(chatResponse);

            // ✅ Only add user input & response to message history for normal chats (not location updates)
            if (!isLocationUpdate)
            {
                messages.Add(new ChatMessage { Content = newText, Role = "user" });
                messages.Add(chatResponse);
            }

            // ✅ Only log responses in the form if this is NOT a location update
            if (!isLocationUpdate && messages.Count > 5 && randomQuery.Count > 0)
            {
                fm.Send(ws.entryIds[formQNum]);
                randomQuery.RemoveAt(0);
            }

            // ✅ Ensure ChatGPT has responded at least once before asking a form question (only for normal chat)
            if (!isLocationUpdate && messages.Count > 3 && randomQuery.Count > 0)
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

    private string GetTimeContext()
    {
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;

        string timeOfDay = "";

        if (hour >= 5 && hour < 12) timeOfDay = "morning";
        else if (hour >= 12 && hour < 17) timeOfDay = "afternoon";
        else if (hour >= 17 && hour < 21) timeOfDay = "evening";
        else timeOfDay = "night";
        return $"The current real-world time is {hour:D2}:{minute:D2}, and it is {timeOfDay}.";
    }
    
    public void NotifyLocationChange(string newLocation)
    {
        currentLocation = newLocation;
        Debug.Log($"Notifying ChatGPT: Entered {currentLocation}");

        // Generate a ChatGPT response based on the new location
        string locationPrompt = $"The player has entered the {currentLocation}. " +
                                $"Generate a line of response that acknowledges this location and provides interesting, relevant thoughts." +
                                "Keep your response short, within one line and do not include emojis.";

        AskChatGPT(locationPrompt, true);
    }
}
