using Rope;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    PlayerController playerController = null!;
    Animator animator = null!;
    PlayerRopeController playerRopeController = null!;
    PlayerRopeStatus playerRopeStatus;
    //HandLSlot handLSlot = null!;
    //HandRSlot handRSlot = null!;

    [SerializeField] private List<Interactable> interactableItems = new List<Interactable>();
    [SerializeField] private Interactable currentInteract;

    private void Awake()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        animator = transform.parent.GetComponentInChildren<Animator>();
        playerRopeController = transform.parent.GetComponent<PlayerRopeController>();
        playerRopeStatus = transform.parent.Find("Status").GetComponent<PlayerRopeStatus>();
    }

    //public void Initialize()
    //{
    //    handLSlot = GetComponentInParent<QuickRefer>().handLSlot;
    //    handRSlot = GetComponentInParent<QuickRefer>().handRSlot;
    //}

    private void OnEnable()
    {
        OnUpdateInteractableItems += updateInteractableItems;
    }

    private void OnDisable()
    {
        OnUpdateInteractableItems -= updateInteractableItems;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (currentInteract == null) return;
        if (playerRopeStatus.ropeStatus == RopeStatus.ConnectedWithClimbAttacher)
        {
            playerRopeController.StartClimbing();
        }
        else
        {
            currentInteract.Interact(playerController);
        }
    }

    public void TryAddInteractable(Interactable item)
    {
        if (!interactableItems.Contains(item))
        {
            interactableItems.Add(item);
            OnUpdateInteractableItems?.Invoke();
        }
    }

    public void TryRemoveInteractable(Interactable item)
    {
        if (interactableItems.Contains(item))
        {
            interactableItems.Remove(item);
            OnUpdateInteractableItems?.Invoke();
        }
    }

    #region Events

    event System.Action OnUpdateInteractableItems;

    private void updateInteractableItems()
    {
        if (interactableItems.Count == 0) return;
        if (interactableItems.Count == 1)
        {
            currentInteract = interactableItems[0];
            return;
        }
        // find the nearest item
    }

    #endregion
}
