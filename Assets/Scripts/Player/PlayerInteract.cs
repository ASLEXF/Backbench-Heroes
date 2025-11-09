# nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    Animator animator = null!;
    GameObject charactorObj = null!;
    //HandLSlot handLSlot = null!;
    //HandRSlot handRSlot = null!;

    [SerializeField] private List<Interactable> interactableItems = null!;
    [SerializeField] private Interactable? currentInteract;

    private void Awake()
    {
        animator = transform.parent.GetComponent<Animator>();
        charactorObj = transform.parent.gameObject;
    }

    //public void Initialize()
    //{
    //    handLSlot = GetComponentInParent<QuickRefer>().handLSlot;
    //    handRSlot = GetComponentInParent<QuickRefer>().handRSlot;
    //}

    private void Update()
    {
        // find the nearest item
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        
    }

    private void OnTriggerEnter(Collider collider)
    {
        //Debug.Log($"{collider.transform.root.gameObject.name}");
        if (collider.gameObject.CompareTag("Interactable"))
        {
            Interactable? interactable = collider.gameObject.GetComponent<Interactable>();
            interactableItems.Add(interactable);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        // handle drop items that are added instantly in the interact range
        
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Interactable"))
        {
            Interactable? interactable = collider.gameObject.GetComponent<Interactable>();
            interactableItems.Remove(interactable);
        }
    }
}
