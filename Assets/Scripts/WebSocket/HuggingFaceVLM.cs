using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HuggingFaceVLM : MonoBehaviour
{
    [Header("Hugging Face Settings")]
    public string apiUrl = "https://router.huggingface.co/v1/chat/completions";
    public string apiKey = "hf_your_token_here";

    [Header("Input")]
    public string imageName = "name_of_image";

    [Header("Output")]
    public TextMeshProUGUI outputTmp;

    void Start()
    {
        // pass
    }

    [Button]
    public void CallHuggingFaceVLM()
    {
        StartCoroutine(SendRequest(imageName)); // Example: put test.jpg in Assets/wwss/
    }

    IEnumerator SendRequest(string imageFileName)
    {
        // Build full path to the image inside Assets/wwss/
        string imagePath = System.IO.Path.Combine(Application.dataPath, "wwss_test_image", imageFileName);

        if (!System.IO.File.Exists(imagePath)) {
            Debug.LogError("Image file not found at path: " + imagePath);
            outputTmp.text = "Image not found.";
            yield break;
        }

        // Load image as byte array and convert to base64
        byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
        string base64Image = Convert.ToBase64String(imageBytes);
        string imageDataUri = $"data:image/jpeg;base64,{base64Image}";

        // Construct JSON payload
        string jsonBody = $@"
    {{
        ""model"": ""Qwen/Qwen2.5-VL-7B-Instruct:hyperbolic"",
        ""messages"": [
            {{
                ""role"": ""user"",
                ""content"": [
                    {{""type"": ""image_url"", ""image_url"": {{ ""url"": ""{imageDataUri}"" }} }},
                    {{""type"": ""text"", ""text"": ""Describe this image in one sentence."" }}
                ]
            }}
        ]
    }}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            string response = request.downloadHandler.text;
            Debug.Log("Response: " + response);
            outputTmp.text = response;
        } else {
            Debug.LogError("Request Error: " + request.error);
            outputTmp.text = "Request Failed";
        }
    }
}
