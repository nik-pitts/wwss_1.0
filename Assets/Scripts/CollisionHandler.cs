using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollisionHandler : MonoBehaviour
{
    public ChatGPTManager chatGPTManager; 

    private void Start()
    {
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
            chatGPTManager.NotifyLocationChange(placeName);
        }
    }
}

