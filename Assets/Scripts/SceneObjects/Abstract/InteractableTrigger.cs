using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableTrigger : MonoBehaviour
{
    Collider trigger;
    [SerializeField] protected Interactable interactable = null!;

    protected virtual void Awake()
    {
        interactable = transform.GetComponentInParent<Interactable>();
        trigger = transform.GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        trigger.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.transform.Find("Interact").GetComponent<PlayerInteract>();
            playerInteract.TryAddInteractable(interactable);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.transform.Find("Interact").GetComponent<PlayerInteract>();
            playerInteract.TryRemoveInteractable(interactable);
        }
    }
}
