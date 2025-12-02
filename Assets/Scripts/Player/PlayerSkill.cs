using Rope;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    PlayerController controller;
    PlayerRopeController ropeController;
    PlayerRopeStatus ropeStatus;

    [SerializeField] bool isUsingSkill = false;

    private void Awake()
    {
        controller = transform.parent.GetComponent<PlayerController>();
        ropeController = transform.parent.GetComponent<PlayerRopeController>();
        ropeStatus = transform.parent.Find("Status").GetComponent<PlayerRopeStatus>();
    }

    public void UseSkill(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (controller.playerType == PlayerType.Archer)
        {
            Debug.Log($"archer used skill");
            if (isUsingSkill)
            {
                isUsingSkill = !ropeController.RetrieveRope();
            }
            else
            {
                ropeController.ThrowRope();
                ropeStatus.ropeStatus = RopeStatus.ConnectedWithClimbAttacher;
                isUsingSkill = true;
            }
                
        }
        else if (controller.playerType == PlayerType.Mage)
        {
            Debug.Log($"mage used skill");
            // Mage skill logic here
        }
        else if (controller.playerType == PlayerType.Warrior)
        {
            Debug.Log($"warrior used skill");
            // Warrior skill logic here
        }
        else
        {             
            Debug.LogWarning("unknown player type");
        }
    }
}
