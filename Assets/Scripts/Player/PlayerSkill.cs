using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : NetworkBehaviour
{
    PlayerController controller;

    private void Awake()
    {
        controller = transform.parent.GetComponent<PlayerController>();
    }

    public void UseSkill(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        // for debug only
        Debug.Log($"player {controller.id} use skill");
    }
}
