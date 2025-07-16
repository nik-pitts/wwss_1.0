using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionManager : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private LayerMask pickUpLayerMask;
    [SerializeField] private Transform objectGrabPointTransform;
    private GamepadControl playerController;
    public int numOfFlowersCollected = 0;
    public int numOfSeedsCollected = 0;

    private ObjectGrabbable objectGrabbable;
    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponentInChildren<GamepadControl>();
    }

    // Update is called once per frame
    void Update()
    {
        float pickUpDistance = 3.0f;
        //Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * pickUpDistance, Color.green);
        
        if (playerController.isRightGrabbed && playerController.isLeftGrabbed)
        {
            if (objectGrabbable == null)
            {
                if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit,
                        pickUpDistance, pickUpLayerMask))
                {
                    if (raycastHit.transform.TryGetComponent(out objectGrabbable))
                    {
                        if (objectGrabbable.isInteractable)
                        {
                            objectGrabbable.Grab(objectGrabPointTransform);
                        }
                    }
                }
            }
        }
        else
        {
            if (objectGrabbable != null)
            {
                objectGrabbable.Drop();
                objectGrabbable = null;
            }
        }
    }

}