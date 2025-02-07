using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private GamepadControl gamepadControl;

    public AudioSource footstepSound;
    // Start is called before the first frame update
    void Start()
    {
        gamepadControl = player.GetComponent<GamepadControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gamepadControl.currentMovement != Vector2.zero)
        {
            footstepSound.enabled = true;
        }
        else
        {
            footstepSound.enabled = false;
        }
    }
}
