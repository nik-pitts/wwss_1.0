using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    private List<ChatMessage> sharedPlaceMessages = new List<ChatMessage>();
    private List<ChatMessage> memories = new List<ChatMessage>();
    private List<int> randomQuery;
    private int formQNum;
    private string dynamicPrompt;
    private MemoryManager memoryManager;

    private void Awake()
    {
        memoryManager = GetComponent<MemoryManager>();
    }

    public void Start()
    {
        string timeContext = GetTimeContext();
        
        string initPrompt = $"You are a friendly boy dog named Shooting Star. " +
                            $"You are a great listener, deeply empathetic, and offer emotional support. " +
                            $"You naturally adapt to the current time and provide appropriate conversation. " +
                            $"If the user has not talked to you today, start with a warm greeting based on the time context: {timeContext}. " +
                            $"Do not repeat the user's response. Instead, use varied vocabulary and expressions. " +
                            $"Do not include emojis in your response.";
        
        string sharedPlaceinitPrompt = $"You are a friendly boy dog named Shooting Star. " +
                                       $"You are a great listener, deeply empathetic, and offer emotional support. " +
                                       $"In this thread you are in the situation walking with your owner. " +
                                       $"Along the way, if you encounter special place then information about the places will be provided." +
                                       $"Based on the place information, generate friendly dialogue recognizing the place that you and your owner share." +
                                       $"Do not include emojis in your response.";
        
        // Random question query for today
        int numQ = ws.formQuestions.Count;
        randomQuery = genRandNums(0, numQ);
        
        // Add initial prompt to the messages list
        ChatMessage promptMessage = new ChatMessage{ Role = "system", Content = initPrompt };
        messages.Add(promptMessage);
        
        // Bring memories if any
        List<ChatMessage> loadedMemories = memoryManager.LoadMemories();
        if (loadedMemories.Count != 0)
        {
            messages.AddRange(loadedMemories);
            Debug.Log($"[ChatGPTManager] Loaded {loadedMemories.Count} past messages into memory.");
        }
        
        // Add initial prompt about shared place dialogue
        ChatMessage sharedPlaceinitMessage = new ChatMessage { Role = "system", Content = sharedPlaceinitPrompt };
        sharedPlaceMessages.Add(sharedPlaceinitMessage);
        
        AskChatGPT("Give warm greetings!", false);
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
            sharedPlaceMessages.Add(new ChatMessage { Content = newText, Role = "user" });
            request.Messages = sharedPlaceMessages;
        }
        else
        {
            messages.Add(new ChatMessage { Content = newText, Role = "user" });
            
            int questionIndex = -1;
            if (messages.Count > 2)
            {
                questionIndex = await FindRelevantFormQuestion(newText);
            }

            if (questionIndex != -1 && randomQuery.Contains(questionIndex))
            {
                // Log response only if relevant
                fm.Send(ws.entryIds[questionIndex]);
                randomQuery.Remove(questionIndex);

                // Modify request to blend form question naturally
                ChatRequestWithFormQuestion(newText, questionIndex);
            }
            else if (messages.Count > 2 && randomQuery.Count > 0)
            {
                // If no relevant response, introduce the next unanswered form question
                int nextQuestionIndex = randomQuery[0];
                ChatRequestWithFormQuestion(newText, nextQuestionIndex);
            }   

            request.Messages = messages;
        }

        request.Model = "gpt-4o-mini";         
        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0) 
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);
            OnResponse.Invoke(chatResponse.Content);
        }
        else
        {
            Debug.LogError("[AskChatGPT] ERROR: No response received from ChatGPT!");
        }
    }
    
    private async Task<int> FindRelevantFormQuestion(string userResponse)
    {
        foreach (var kvp in ws.formQuestions)
        {
            int questionIndex = kvp.Key;
            string question = kvp.Value;

            if (randomQuery.Contains(questionIndex)) // only check unanswered questions
            {
                string prompt = $"Does the following player response answer this question?" +
                                $"Question: {question}" +
                                $"Player Response: {userResponse}" +
                                $"Answer with ONLY 'yes' or 'no'.";
                Debug.Log(prompt);
                var checkRequest = new CreateChatCompletionRequest
                {
                    Messages = new List<ChatMessage> { new ChatMessage { Content = prompt, Role = "user" } },
                    Model = "gpt-4o-mini"
                };

                var checkResponse = await openAI.CreateChatCompletion(checkRequest);
                if (checkResponse.Choices != null && checkResponse.Choices.Count > 0)
                {
                    string answer = checkResponse.Choices[0].Message.Content.Trim().ToLower();
                    string alphabetOnly = Regex.Replace(answer, "[^a-zA-Z]", "");
                    Debug.Log(alphabetOnly);
                    if (alphabetOnly == "yes")
                    {
                        Debug.Log($"Relevant response to question: {question}");
                        StoreMemory(question, userResponse);
                        return questionIndex; // return index instead of string
                    }
                    else
                    {
                        Debug.Log($"Irrelevant response to question: {question}");
                    }
                }
            }
        }
        return -1; // no matching question found
    }

    private void StoreMemory(string question, string userResponse)
    {
        string questionResponse = $"To this {question}, user responded {userResponse}.";
        ChatMessage questionResponseMsg = new ChatMessage { Content = questionResponse, Role = "user" };
        memories.Add(questionResponseMsg);
    }

    private void ChatRequestWithFormQuestion(string userResponse, int qNum)
    {
        String formQToAdd = ws.formQuestions[qNum];

        String instruction = $"Considering that you should ask {formQToAdd} to player," +
                             $"formulate your next response to naturally go well with {userResponse}";

        ChatMessage reformulatedRequest = new ChatMessage();
        reformulatedRequest.Content = instruction;
        reformulatedRequest.Role = "user";

        messages.Add(reformulatedRequest);
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
        string locationPrompt = $"Player has entered the {currentLocation}. " +
                                $"Generate a line of response that acknowledges this location and provides interesting, relevant thoughts," +
                                $"that you can share with your owner to smooth the conversation and natural." +
                                $"Keep your response short, within a line.";

        AskChatGPT(locationPrompt, true);
    }
    public void SaveAndExit()
    {
        memoryManager.SaveMemories(memories);
        Debug.Log("[ChatGPTManager] Saving memories and exiting.");
        Application.Quit();
    }
}
