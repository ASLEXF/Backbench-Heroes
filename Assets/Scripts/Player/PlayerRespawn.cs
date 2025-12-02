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

    private void Update()
    {
        if (transform.position.y < -10)
            Respawn();
    }

    private void OnEnable()
    {
        //if (SpawnPoint == null)
        //    SpawnPoint = GameObject.Find("Spawn Point").transform;
    }

    public void Respawn()
    {
        if (SpawnPoint == null)
            return;

        // find the nearest enabled spawn point

        controller.MoveTo(SpawnPoint.position);
        //controller.Respawn();
        //playerHealth.Respawn();
    }
}
