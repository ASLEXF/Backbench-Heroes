using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    private static int PlayerId = 1;
    public int id = 0;

    GameObject playerObj;
    GameObject cameraRoot;
    Rigidbody playerRb;
    PlayerAttack playerAttack;
    PlayerAttacked playerAttacked;
    PlayerInteract playerInteract;
    PlayerSkill playerSkill;
    Camera playerCamera;

    NetworkTransform networkTransform;
    NetworkAnimator networkAnimator;
    private NetworkVariable<Quaternion> modelRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [Space(10)]
    [SerializeField] float walkSpeed = 3.0f;
    [SerializeField] float runSpeed = 8.0f;
    [SerializeField] float inAirSpeed = 5.0f;
    [SerializeField] float deceleration = 0.6f;
    [SerializeField] float acceleration = 4f;
    [SerializeField] float currentSpeed;
    public Vector3 playerVelocity = new Vector3();  // for movement from outside
    float _verticalVelocity;

    [Space(10)]
    [SerializeField] float gravity = -19.0f;
    [SerializeField] float JumpHeight = 4.0f;
    [SerializeField] bool Grounded = false;
    [SerializeField] LayerMask GroundLayers;
    [SerializeField] float GroundedOffset = -0.14f;
    [SerializeField] float GroundedRadius = 0.28f;

    [Space(10)]
    //[SerializeField] float rotationSmoothTime = 0.5f;
    private float _rotationVelocity;
    private float _rotateY;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerObj = transform.GetChild(0).gameObject;
        cameraRoot = transform.Find("Camera Root").gameObject;

        if (IsOwner)
        {
            playerRb = GetComponent<Rigidbody>();
            playerAttack = playerObj.GetComponent<PlayerAttack>();
            playerAttacked = playerObj.GetComponent<PlayerAttacked>();
            playerSkill = playerObj.GetComponent<PlayerSkill>();
            playerInteract = transform.Find("Interact").GetComponent<PlayerInteract>();
            playerCamera = cameraRoot.GetComponentInChildren<Camera>();
            networkAnimator = playerObj.GetComponent<NetworkAnimator>();

            cameraRoot.SetActive(true);

            initialInputAction();
            RequestIdServerRpc();
        }
        else
        {
            cameraRoot.SetActive(false);

            if (playerInput != null)
            {
                playerInput.Disable();
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        GroundedCheck();

        playerVelocity = playerRb.velocity;
    }

    private void FixedUpdate()
    {
        handleMovement();
        handleRotation();
    }

    private void handleMovement()
    {
        if (!IsOwner) return;

        //move = (transform.right * _rawInputMovement.x + transform.forward * _rawInputMovement.y).normalized;
        Vector3 move = getMove(_rawInputMovement);

        if (move == Vector3.zero)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            if (!_isRunning)
            {
                if (currentSpeed > walkSpeed)
                    currentSpeed = Mathf.MoveTowards(currentSpeed, walkSpeed, acceleration * Time.fixedDeltaTime);
                else
                    currentSpeed = Mathf.MoveTowards(currentSpeed, walkSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                if (currentSpeed < walkSpeed)
                    currentSpeed = walkSpeed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, runSpeed, acceleration * Time.fixedDeltaTime);
            }
        }

        Vector3 velocity = playerRb.velocity;
        Vector3 horizontal = (move * (Grounded ? currentSpeed : inAirSpeed));
        velocity.x = horizontal.x;
        velocity.y = _verticalVelocity;
        velocity.z = horizontal.z;

        playerRb.velocity = velocity;

        SubmitTransformServerRpc(transform.position, transform.rotation);

        networkAnimator.Animator.SetFloat("Speed", currentSpeed);
    }

    private void handleRotation()
    {
        if (IsOwner)
        {
            Vector3 move = getMove(_rawInputMovement);
            // turn the player
            _rotateY = transform.eulerAngles.y;
            //float rotation = Mathf.SmoothDampAngle(_rotateY, Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg, ref _rotationVelocity, rotationSmoothTime);
            float rotation = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;  // NOT SMOOTH
            playerObj.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            modelRotation.Value = playerObj.transform.rotation;
        }
        else
        {
            playerObj.transform.rotation = modelRotation.Value;
        }
    }

    private bool _lastGrounded = false;

    private void GroundedCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.down * GroundedOffset;
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        if (Grounded != _lastGrounded)
        {
            networkAnimator.Animator.SetBool("IsGrounded", Grounded);
            _lastGrounded = Grounded;
        }

        if (!Grounded)
            _verticalVelocity += gravity * Time.deltaTime;
        else if (_verticalVelocity < 0f)
            _verticalVelocity = -2f;
    }

    private Vector3 getMove(Vector3 _rawInputMovement)
    {
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return camForward * _rawInputMovement.y + camRight * _rawInputMovement.x;
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }

    #region RPC

    [ServerRpc]
    private void SubmitTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    [ServerRpc]
    private void RequestIdServerRpc(ServerRpcParams rpcParams = default)
    {
        id = PlayerController.PlayerId++;
    }

    #endregion

    #region Input System

    //InputAction move, run, attack, block, interact;
    private Player playerInput;
    Vector2 _rawInputMovement;

    bool _isRunning = false;

    void initialInputAction()
    {
        playerInput = new Player();

        // use rebind manager here if players can rebind their key bindings
        if (PlayerId < 4)
        {
            playerInput.InGame.Move.started += handleAction;
            playerInput.InGame.Move.performed += handleAction;
            playerInput.InGame.Move.canceled += handleAction;
            playerInput.InGame.Run.performed += handleAction;
            playerInput.InGame.Jump.canceled += handleAction;
            playerInput.InGame.Attack.started += handleAction;
            playerInput.InGame.Skill.started += handleAction;
            playerInput.InGame.Skill.performed += handleAction;
            playerInput.InGame.Skill.canceled += handleAction;
            playerInput.InGame.Interact.started += handleAction;

            playerInput.Enable();
        }
        else
            Debug.LogError($"wrong player input initialization: id={PlayerId}.");

    }

    void handleAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        switch (context.action.name)
        {
            case "Move":
                _rawInputMovement = context.ReadValue<Vector2>();
                networkAnimator.Animator.SetBool("IsWalking", context.performed);
                break;
            case "Run":
                _isRunning = true; // context.performed;
                break;
            case "Jump":
                if (Grounded) // && context.canceled)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * gravity);
                    networkAnimator.SetTrigger("Jump");
                }
                break;
            case "Attack":
                playerAttack.Attack(context);
                break;
            case "Skill":
                playerSkill.UseSkill(context);
                break;
            case "Interact":
                playerInteract.Interact(context);
                break;
            default:
                Debug.LogWarning($"undefined action {context.action.name}!");
                break;
        }
    }

    #endregion
}
