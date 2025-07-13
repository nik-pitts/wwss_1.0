using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialFill : MonoBehaviour
{
    [SerializeField] private Image flowerImageUI;
    [SerializeField] private Image seedImageUI;
    [SerializeField] private float fillSpeed = 0.5f;
    [SerializeField] private GameObject player;

    private int totalNumofFlowers = 6;
    private int totalNumofSeeds = 6;
    void Start()
    {
        flowerImageUI.fillAmount = 0f; // Initialize the radial fill to 0
        seedImageUI.fillAmount = 0f; // Initialize the radial fill to 0
    }

    // Update is called once per frame
    void Update()
    {
        int numOfFlowersCollected = player.GetComponent<PlayerInteractionManager>().numOfFlowersCollected;
        float flowerTargetFillAmount = (float)numOfFlowersCollected / totalNumofFlowers;
        
        // Smoothly lerp to the target fill amount
        flowerImageUI.fillAmount = Mathf.Lerp(flowerImageUI.fillAmount, flowerTargetFillAmount, fillSpeed * Time.deltaTime);

        int numOfSeedsCollected = player.GetComponent<PlayerInteractionManager>().numOfSeedsCollected;
        float seedTargetFillAmount = (float)numOfSeedsCollected / totalNumofSeeds;

        // Smoothly lerp to the target fill amount
        seedImageUI.fillAmount = Mathf.Lerp(seedImageUI.fillAmount, seedTargetFillAmount, fillSpeed * Time.deltaTime);
    }
}
