using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    Animator animator = null!;
    PlayerAttacked playerAttacked = null!;
    //HandLSlot handLSlot = null!;
    //HandRSlot handRSlot = null!;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerAttacked = GetComponent<PlayerAttacked>();
    }

    //public void Initialize()
    //{
    //    handLSlot = transform.parent.GetComponent<QuickRefer>().handLSlot;
    //    handLSlot.Initialize();
    //    handRSlot = transform.parent.GetComponent<QuickRefer>().handRSlot;
    //    handRSlot.Initialize();
    //}

    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        //if (handRSlot.GetCurrentWeaponObj() == null) return;

        animator.SetTrigger("Attack");
    }
}
