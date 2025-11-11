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
        SpawnPoint = GameObject.Find("Spawn Point").transform;
        //playerHealth = GetComponent<PlayerHealth>();
        //playerAttacked = transform.parent.GetChild(0).GetComponent<PlayerAttacked>();
    }

    private void Update()
    {
        if (transform.position.y < -10)
            Respawn();
    }

    public void Respawn()
    {
        controller.MoveTo(SpawnPoint.position);
        //controller.Respawn();
        //playerHealth.Respawn();
    }
}
