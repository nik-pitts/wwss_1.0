using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject rockContainer;
    [SerializeField] private GameObject flowerContainer;
    [SerializeField] private GameObject seedContainer;
    [SerializeField] private GameObject seedBoxObject;
    [SerializeField] private Transform handPosition;
    [SerializeField] private GameObject seed;
    [SerializeField] private GameObject jar;
    private Transform[] rocks;
    private Transform[] flowers;
    private Transform[] seeds;
    private Transform seedBox;
    public bool isNearRock;
    public bool isNearFlower;
    public bool isNearSeed;
    public bool isNearSeedBox = false;
    public bool isNearJar = false;
    private bool wasNearFlower = false;
    private bool wasNearSeedBox = false;
    private bool hasAlreadyPoured = false;
    private Transform currentFlower = null; // Track which flower we're interacting with
    private float interactableDistance = 2f;
    private float flowerInteractableDistance = 4f;
    private float seedInteractableDistance = 2f;


    public bool isHoldingRock = false;
    public bool isHoldingFlower = false;
    public bool isHoldingSeed = false;
    public bool isHoldingJar = false;
    private GamepadControl playerController;
    public int numOfFlowersCollected = 0;
    public int numOfSeedsCollected = 0;

    // Start is called before the first frame update
    void Start()
    {
        rocks = rockContainer.GetComponentsInChildren<Transform>();
        flowers = flowerContainer.GetComponentsInChildren<Transform>();
        seeds = seedContainer.GetComponentsInChildren<Transform>();
        seedBox = seedBoxObject.GetComponent<Transform>();
        playerController = GetComponentInChildren<GamepadControl>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIsNearRock();
        CheckIsNearFlower();
        CheckIsNearSeed();
        CheckIsNearJar();
        CheckIsNearSeedBox();

        // Clear text when moving away from flower
        if (wasNearFlower && !isNearFlower && currentFlower != null)
        {
            ClearFlowerText(currentFlower);
            currentFlower = null;
        }
        wasNearFlower = isNearFlower;
        wasNearSeedBox = isNearSeedBox;
    }

    void CheckIsNearRock()
    {
        isNearRock = false;
        if (rocks.Length == 0) return;

        foreach (Transform rock in rocks)
        {
            Vector2 rockPosition = new Vector2(rock.position.x, rock.position.z);
            Vector2 playerPosition = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(playerPosition, rockPosition);
            if (distance < interactableDistance)
            {
                isNearRock = true;
                HandleRockInteraction(rock);
                break;
            }
        }
    }

    void CheckIsNearFlower()
    {
        isNearFlower = false;
        if (flowers.Length == 0) return;

        foreach (Transform flower in flowers)
        {
            if (flower == null) continue; // Skip if flower is null
            Vector2 flowerPosition = new Vector2(flower.position.x, flower.position.z);
            Vector2 playerPosition = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(playerPosition, flowerPosition);
            if (distance < flowerInteractableDistance)
            {
                isNearFlower = true;
                currentFlower = flower; // Store reference to current flower
                HandleFlowerInteraction(flower);
                break;
            }
        }
    }

    void CheckIsNearSeed()
    {
        isNearSeed = false;
        if (seeds.Length == 0) return;

        foreach (Transform seed in seeds)
        {
            if (seed == null) continue; // Skip if flower is null
            Vector2 seedPosition = new Vector2(seed.position.x, seed.position.z);
            Vector2 playerPosition = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(playerPosition, seedPosition);
            if (distance < seedInteractableDistance)
            {
                isNearSeed = true;
                HandleSeedInteraction(seed);
                break;
            }
        }
    }

    void CheckIsNearSeedBox()
    {
        if (seedBox == null) return;

        Vector2 seedBoxPosition = new Vector2(seedBox.position.x, seedBox.position.z);
        Vector2 playerPosition = new Vector2(transform.position.x, transform.position.z);
        float distance = Vector2.Distance(playerPosition, seedBoxPosition);
        if (distance < 4.0f)
        {
            isNearSeedBox = true;
            HandleSeedBoxInteraction(seedBox);
        }
        else
        {
            isNearSeedBox = false;
            ClearSeedBoxText(seedBox);
        }
    }

    void CheckIsNearJar()
    {
        isNearJar = false;
        if (jar == null) return;

        Vector2 jarPosition = new Vector2(jar.transform.position.x, jar.transform  .position.z);
        Vector2 playerPosition = new Vector2(transform.position.x, transform.position.z);
        float distance = Vector2.Distance(playerPosition, jarPosition);
        if (distance < interactableDistance)
        {
            isNearJar = true;
            HandleJarInteraction(jar.transform);
        }
    }   

    void HandleRockInteraction(Transform rock)
    {
        if (!rock.GetComponent<RockBehavior>().isInteractable)
        {
            return; // Skip interaction if rock is not interactable
        }
        if (playerController.isLeftGrabbed && playerController.isRightGrabbed)
        {
            if (isHoldingRock) return; // Already holding a rock
            Rigidbody rockRigidbody = rock.GetComponent<Rigidbody>();
            if (rockRigidbody != null)
            {
                rockRigidbody.isKinematic = true;
                rockRigidbody.useGravity = false;
            }
            rock.SetParent(handPosition);
            isHoldingRock = true;
            rock.localPosition = Vector3.zero;
        }
        else
        {
            // Create UI elements to show how to interact with the rock
            // For example, show a tooltip or highlight the rock

            // Drop the rock
            Rigidbody rockRigidbody = rock.GetComponent<Rigidbody>();
            if (rockRigidbody != null)
            {
                rockRigidbody.isKinematic = false; // Enable physics
                rockRigidbody.useGravity = true; // Enable gravity
            }

            rock.SetParent(rockContainer.transform, true);
            isHoldingRock = false;
        }
    }

    void HandleFlowerInteraction(Transform flower)
    {
        if (!(playerController.isLeftGrabbed && playerController.isRightGrabbed))
        {
            if (!isHoldingFlower)
            {
                MeshRenderer leftShoulder = flower.Find("left_shoulder").GetComponent<MeshRenderer>();
                MeshRenderer rightShoulder = flower.Find("right_shoulder").GetComponent<MeshRenderer>();

                if (leftShoulder != null)
                    leftShoulder.enabled = true;

                if (rightShoulder != null)
                    rightShoulder.enabled = true;
            }
            else
            {
                // Drop the flower
                numOfFlowersCollected += 1;
                // Remove the flower from the hand slowly
                Destroy(flower.gameObject, 2f);
                isHoldingFlower = false;
            }
        }
        else
        {
            ClearFlowerText(flower);
            if (isHoldingFlower) return; // Already holding a flower
            Rigidbody flowerRigidbody = flower.GetComponent<Rigidbody>();
            if (flowerRigidbody != null)
            {
                flowerRigidbody.isKinematic = true;
                flowerRigidbody.useGravity = false;
            }
            flower.SetParent(handPosition);
            isHoldingFlower = true;
            flower.localPosition = Vector3.zero;
        }
    }

    void ClearFlowerText(Transform flower)
    {
        MeshRenderer leftShoulder = flower.Find("left_shoulder").GetComponent<MeshRenderer>();
        MeshRenderer rightShoulder = flower.Find("right_shoulder").GetComponent<MeshRenderer>();

        if (leftShoulder != null)
        {
            leftShoulder.enabled = false;
        }

        if (rightShoulder != null)
        {
            rightShoulder.enabled = false;
        }
    }

    void HandleSeedInteraction(Transform seed)
    {
        if (!(playerController.isLeftGrabbed && playerController.isRightGrabbed))
        {
            if (isHoldingSeed)
            {
                numOfSeedsCollected += 1;
                // Remove the seed from the hand slowly
                Destroy(seed.gameObject, 2f);
                isHoldingSeed = false;
            }
        }
        else
        {
            if (isHoldingSeed) return; // Already holding a seed
            Rigidbody seedRigidbody = seed.GetComponent<Rigidbody>();
            if (seedRigidbody != null)
            {
                seedRigidbody.isKinematic = true;
                seedRigidbody.useGravity = false;
            }
            seed.SetParent(handPosition);
            isHoldingSeed = true;
            seed.localPosition = Vector3.zero;
        }
    }

    void HandleSeedBoxInteraction(Transform seedBox)
    {
        if (playerController.isPouring && !hasAlreadyPoured)
        {
            Vector3 seedBoxPosition = seedBox.position;
            for (int i = 0; i < numOfSeedsCollected; i++)
            {
                GameObject newSeed = Instantiate(seed, seedBoxPosition, Quaternion.identity);
                newSeed.GetComponent<SphereCollider>().isTrigger = false;
                newSeed.GetComponent<Rigidbody>().isKinematic = false;
                newSeed.GetComponent<Rigidbody>().useGravity = true;
                newSeed.transform.SetParent(seedBox);
            }
            numOfSeedsCollected = 0;
            hasAlreadyPoured = true;
            Debug.Log("Player is pouring");
        }

        // Reset flag when not pouring
        if (!playerController.isPouring)
        {
            hasAlreadyPoured = false;
        }
        if (!wasNearSeedBox)
        {
            Debug.Log("Handle Seed Box Interaction Called");
            MeshRenderer b = seedBox.Find("B").GetComponent<MeshRenderer>();

            if (b != null)
                b.enabled = true;
        }
    }

    void ClearSeedBoxText(Transform seedBox)
    {
        if (wasNearSeedBox)
        {
            MeshRenderer b = seedBox.Find("B").GetComponent<MeshRenderer>();

            if (b != null)
                b.enabled = false;
        }
    }
    
    void HandleJarInteraction(Transform jar)
    {
        if (playerController.isLeftGrabbed && playerController.isRightGrabbed)
        {
            if (isHoldingJar) return; // Already holding a rock
            Rigidbody jarRigidbody = jar.Find("Jar").GetComponent<Rigidbody>();
            if (jarRigidbody != null)
            {
                jarRigidbody.isKinematic = true;
                jarRigidbody.useGravity = false;
            }
            jar.SetParent(handPosition);
            isHoldingJar = true;
            jar.localPosition = Vector3.zero;
        }
        else
        {
            // Create UI elements to show how to interact with the rock
            // For example, show a tooltip or highlight the rock

            // Drop the jar
            Rigidbody jarRigidbody = jar.Find("Jar").GetComponent<Rigidbody>();
            if (jarRigidbody != null)
            {
                jarRigidbody.isKinematic = false; // Enable physics
                jarRigidbody.useGravity = true; // Enable gravity
            }

            jar.SetParent(null);
            isHoldingJar = false;
        }
    }   
}