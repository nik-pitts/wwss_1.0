using UnityEngine;
using System;

public class SkyboxController : MonoBehaviour
{
    [Header("Skybox Materials")]
    [SerializeField] private Material morningSkybox;
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material eveningSkybox;
    [SerializeField] private Material nightSkybox;
    
    [Header("Fog Colors")]
    [SerializeField] private Color morningFogColor = new Color(0.8f, 0.6f, 0.5f); 
    [SerializeField] private Color dayFogColor = new Color(0.7f, 0.8f, 1f);       
    [SerializeField] private Color eveningFogColor = new Color(0.6f, 0.3f, 0.2f); 
    [SerializeField] private Color nightFogColor = new Color(0.05f, 0.05f, 0.1f); 

    private void Start()
    {
        UpdateSkybox();
        InvokeRepeating("UpdateSkybox", 60f, 60f); // Check and update every 60 seconds
    }

    private void UpdateSkybox()
    {
        // Get real-world time
        int hour = DateTime.Now.Hour;
        Debug.Log(hour);

        // Change skybox based on time of day
        if (hour >= 6 && hour < 12) // Morning
        {
            RenderSettings.skybox = morningSkybox;
            RenderSettings.fogColor = morningFogColor;
        }
        else if (hour >= 12 && hour < 18) // Day
        {
            RenderSettings.skybox = daySkybox;
            RenderSettings.fogColor = dayFogColor;
        }
        else if (hour >= 18 && hour < 21) // Evening
        {
            RenderSettings.skybox = eveningSkybox;
            RenderSettings.fogColor = eveningFogColor;
        }
        else // Night
        {
            RenderSettings.skybox = nightSkybox;
            RenderSettings.fogColor = nightFogColor;
        }
    }
}