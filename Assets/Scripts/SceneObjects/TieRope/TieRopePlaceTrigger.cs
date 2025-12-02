using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TieRopePlaceTrigger : InteractableTrigger
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TieRopePlace Detected Player");
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.playerType == PlayerType.Archer)
            {
                PlayerInteract playerInteract = other.transform.Find("Interact").GetComponent<PlayerInteract>();
                playerInteract.TryAddInteractable(interactable);
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TieRopePlace Detected Player Exit");
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.playerType == PlayerType.Archer)
            {
                PlayerInteract playerInteract = other.transform.Find("Interact").GetComponent<PlayerInteract>();
                playerInteract.TryRemoveInteractable(interactable);
            }
        }
    }
}
