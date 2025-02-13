using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollisionHandler : MonoBehaviour
{
    public ChatGPTManager chatGPTManager; // Reference to ChatGPTManager

    private void Start()
    {
        // Find ChatGPTManager automatically if not assigned
        if (chatGPTManager == null)
        {
            chatGPTManager = FindObjectOfType<ChatGPTManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        string placeName = "";

        if (other.CompareTag("Foresty"))
        {
            placeName = "Forest";
        }
        else if (other.CompareTag("Rocky"))
        {
            placeName = "Rocky Terrain";
        }
        else if (other.CompareTag("Lighthouse"))
        {
            placeName = "Lighthouse";
        }

        if (!string.IsNullOrEmpty(placeName))
        {
            Debug.Log($"Entered {placeName} zone!");
            chatGPTManager.NotifyLocationChange(placeName); // Send info to ChatGPT
        }
    }
}

