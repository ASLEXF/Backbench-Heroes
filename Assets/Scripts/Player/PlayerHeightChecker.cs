using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeightChecker : MonoBehaviour
{
    [SerializeField] float height = -10.0f;
    //PlayerHealth playerHealth;
    PlayerRespawn playerRespawn;

    private void Awake()
    {
        //playerHealth = transform.parent.GetComponentInChildren<PlayerHealth>();
        playerRespawn = transform.parent.GetComponentInChildren<PlayerRespawn>();
    }

    private void FixedUpdate()
    {
        if (transform.position.y < height)
        {
            //playerHealth.Die();
            playerRespawn.Respawn();
        }
    }
}
