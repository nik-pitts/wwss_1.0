using System;
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
        else if (other.CompareTag("Haystick"))
        {
            placeName = "Area with piles of hay stick";
            Debug.Log("Haystick detected");

        }
        else if (other.CompareTag("Water"))
        {
            placeName = "Water";
            Debug.Log("Waterbed detected");
        }

        if (!string.IsNullOrEmpty(placeName))
        {
            if (Context.Instance.GetNeedApiInput)
            {
                chatGPTManager.NotifyLocationChange(placeName);
            }
        }
    }
}

