using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private GamepadControl gamepadControl;

    [SerializeField] private AudioSource footstepWalk;
    [SerializeField] private AudioSource footstepRun;

    void Start()
    {
        gamepadControl = player.GetComponent<GamepadControl>();
        if (footstepWalk == null || footstepRun == null)
        {
            Debug.LogError("Footstep Audio Source Not Assigned.");
            enabled = false;
        }
    }

    void Update()
    {
        if (gamepadControl.currentMovement != Vector2.zero)
        {
            HandleFootsteps();
        }
        else
        {
            StopFootsteps();
        }
    }

    void HandleFootsteps()
    {
        if (gamepadControl.isRunning)
        {
            footstepRun.enabled = true;
            footstepWalk.enabled = false;
        }
        else
        {
            footstepRun.enabled = false;
            footstepWalk.enabled = true;
        }
    }

    void StopFootsteps()
    {
        footstepWalk.enabled = false;
        footstepRun.enabled = false;
    }
}
