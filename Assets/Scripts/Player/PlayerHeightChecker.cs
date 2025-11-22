using UnityEngine;

public class PlayerHeightChecker : MonoBehaviour
{
    [SerializeField] float height = -10.0f;
    //PlayerHealth playerHealth;
    PlayerRespawn playerRespawn;

    private void Awake()
    {
        //playerHealth = GetComponent<PlayerHealth>();
        playerRespawn = GetComponent<PlayerRespawn>();
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
