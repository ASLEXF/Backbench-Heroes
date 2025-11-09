using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    PlayerController controller;
    //PlayerHealth playerHealth;
    //PlayerAttacked playerAttacked;

    [SerializeField] public Transform SpawnPoint;

    private void Awake()
    {
        controller = transform.parent.GetComponent<PlayerController>();
        //playerHealth = GetComponent<PlayerHealth>();
        //playerAttacked = transform.parent.GetChild(0).GetComponent<PlayerAttacked>();
    }

    public void Respawn()
    {
        controller.MoveTo(SpawnPoint.position);
        //controller.Respawn();
        //playerHealth.Respawn();
    }
}
