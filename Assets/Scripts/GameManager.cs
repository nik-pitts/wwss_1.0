using System.Collections;
using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Level Management")]
    [SerializeField] private bool _isNextLevel = false;
    public bool isNextLevel 
    { 
        get { return _isNextLevel; } 
        set 
        { 
            if (value && !_isNextLevel && Application.isPlaying) // Prevent infinite loop
            { 
                _isNextLevel = value;
                StartTransition();
            }
            else
            {
                _isNextLevel = value;
            }
        } 
    }
    public int currentLevel = 1;
    
    [Header("Player")]
    public GameObject player;
    public Transform[] levelSpawnPoints; // Array of spawn points for each level
    
    [Header("Fade Effect")]
    public Image fadeImage; // Black UI image for fade effect
    public float fadeSpeed = 1f;
    
    [Header("Level Objects")]
    public GameObject[] levelTerrains; // Array of your 3 level terrains
    
    [Header("Debug/Testing")]
    [SerializeField] private bool triggerNextLevel = false; // Inspector button
    
    // Singleton pattern for easy access
    public static GameManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Start with fade in
        StartCoroutine(FadeIn());
        
        // Activate only the first level
        SetActiveLevel(currentLevel);
    }
    
    // This gets called when Inspector values change
    private void OnValidate()
    {
        if (Application.isPlaying && _isNextLevel)
        {
            StartTransition();
        }
    }
    
    private void Update()
    {
        // Debug: Check for manual trigger in inspector
        if (triggerNextLevel)
        {
            triggerNextLevel = false; // Reset the flag
            CompleteLevel();
        }
    }
    
    public void CompleteLevel()
    {
        if (currentLevel < levelTerrains.Length)
        {
            StartTransition();
        }
        else
        {
            // All levels completed
            Debug.Log("All levels completed!");
        }
    }
    
    private void StartTransition()
    {
        if (currentLevel < levelTerrains.Length && !isTransitioning)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }
    
    private bool isTransitioning = false; // Prevent multiple transitions
    
    private IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;
        
        // 1. Fade to black
        yield return StartCoroutine(FadeOut());
        
        // 2. Move to next level
        currentLevel++;
        
        // 3. Deactivate current level, activate next level
        SetActiveLevel(currentLevel);
        
        // 4. Respawn player at new position
        RespawnPlayer();
        
        // 5. Fade back in
        yield return StartCoroutine(FadeIn());
        
        // 6. Reset the flags
        _isNextLevel = false;
        isTransitioning = false;
    }
    
    private void SetActiveLevel(int level)
    {
        // Deactivate all levels
        for (int i = 0; i < levelTerrains.Length; i++)
        {
            levelTerrains[i].SetActive(false);
        }
        
        // Activate the current level (array is 0-indexed, levels are 1-indexed)
        if (level - 1 < levelTerrains.Length)
        {
            levelTerrains[level - 1].SetActive(true);
        }
    }
    
    private void RespawnPlayer()
    {
        if (player != null && currentLevel - 1 < levelSpawnPoints.Length)
        {
            // Disable player controller temporarily
            var playerController = player.GetComponentInChildren<GamepadControl>();
            var playerMove = player.GetComponentInChildren<PlayerMove>();

            if (playerController != null && playerMove != null)
            {
                playerController.enabled = false;
                playerMove.enabled = false;
            }
            
            // Move player to spawn point
            player.transform.position = levelSpawnPoints[currentLevel - 1].position;
            player.transform.rotation = levelSpawnPoints[currentLevel - 1].rotation;
            
            // Reset player physics
            var playerRigidbody = player.GetComponentInChildren<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            
            // Re-enable player controller
            if (playerController != null && playerMove != null)
            {
                playerController.enabled = true;
                playerMove.enabled = true;
            }
        }
    }
    
    private IEnumerator FadeOut()
    {
        float alpha = 0f;
        fadeImage.color = new Color(0, 0, 0, alpha);
        
        while (alpha < 1f)
        {
            alpha += fadeSpeed * Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1f);
    }
    
    private IEnumerator FadeIn()
    {
        float alpha = 1f;
        fadeImage.color = new Color(0, 0, 0, alpha);
        
        while (alpha > 0f)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 0f);
    }
}