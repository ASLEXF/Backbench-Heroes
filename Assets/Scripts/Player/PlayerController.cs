using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static int PlayerId = 1;
    public int id = 0;

    public GameObject playerObj { get; private set; }
    GameObject cameraRoot;

    public Rigidbody rb { get; private set; }
    PlayerAttack playerAttack;
    PlayerAttacked playerAttacked;
    PlayerInteract playerInteract;
    PlayerSkill playerSkill;
    PlayerRespawn playerRespawn;
    public Animator animator { get; private set; }
    public Camera playerCamera { get; private set; }
    PlayerRopeStatus playerRopeStatus;

    public PlayerType playerType;

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
    public bool Grounded = false;
    [SerializeField] LayerMask GroundLayers;
    [SerializeField] float GroundedOffset = -0.14f;
    [SerializeField] float GroundedRadius = 0.28f;

    [Space(10)]
    //[SerializeField] float rotationSmoothTime = 0.5f;
    private float _rotationVelocity;
    private float _rotateY;

    private void Awake()
    {
        playerObj = transform.GetChild(0).gameObject;
        cameraRoot = transform.Find("Camera Root").gameObject;

        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerAttack = playerObj.GetComponent<PlayerAttack>();
        playerAttacked = playerObj.GetComponent<PlayerAttacked>();
        playerSkill = playerObj.GetComponent<PlayerSkill>();
        playerInteract = transform.Find("Interact").GetComponent<PlayerInteract>();
        playerRespawn = transform.Find("Status").GetComponent<PlayerRespawn>();
        playerCamera = cameraRoot.GetComponentInChildren<Camera>();
        animator = playerObj.GetComponent<Animator>();
        playerRopeStatus = transform.Find("Status").GetComponent<PlayerRopeStatus>();
    }

    private void Start()
    {
        playerRespawn.Respawn();
        cameraRoot.SetActive(true);
        BindInputAction();
    }

    private void Update()
    {
        GroundedCheck();

        playerVelocity = rb.velocity;
    }

    private void FixedUpdate()
    {
        handleMovement();
        handleRotation();
    }

    private void handleMovement()
    {
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

        Vector3 velocity = rb.velocity;
        Vector3 horizontal = (move * (Grounded ? currentSpeed : inAirSpeed));
        velocity.x = horizontal.x;
        velocity.y = _verticalVelocity;
        velocity.z = horizontal.z;

        rb.velocity = velocity;

        animator.SetFloat("Speed", currentSpeed);
    }

    private void handleRotation()
    {
        Vector3 move = getMove(_rawInputMovement);
        if (move == Vector3.zero)
            return;
        // turn the player
        _rotateY = transform.eulerAngles.y;
        //float rotation = Mathf.SmoothDampAngle(_rotateY, Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg, ref _rotationVelocity, rotationSmoothTime);
        float rotation = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;  // NOT SMOOTH
        playerObj.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    }

    private bool _lastGrounded = false;

    public void GroundedCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.down * GroundedOffset;
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        if (Grounded != _lastGrounded)
        {
            animator.SetBool("IsGrounded", Grounded);
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

    #region quick accessors

    CapsuleCollider capsuleCollider;

    public float radius { get; private set; }

    private void initializeQuickAccessors()
    {
        radius = capsuleCollider.radius;
    }

    #endregion

    #region Input System

    //InputAction move, run, attack, block, interact;
    public PlayerInput playerInput;
    public PlayerInputs playerInputs;
    Vector2 _rawInputMovement;

    bool _isRunning = false;

    public void BindInputAction()
    {
        playerInputs = new PlayerInputs();

        // use rebind manager here if players can rebind their key bindings
        if (PlayerId < 4)
        {
            playerInputs.InGame.Move.started += handleAction;
            playerInputs.InGame.Move.performed += handleAction;
            playerInputs.InGame.Move.canceled += handleAction;
            playerInputs.InGame.Run.performed += handleAction;
            playerInputs.InGame.Jump.started += handleAction;
            playerInputs.InGame.Attack.started += handleAction;
            playerInputs.InGame.Skill.started += handleAction;
            playerInputs.InGame.Skill.performed += handleAction;
            playerInputs.InGame.Skill.canceled += handleAction;
            playerInputs.InGame.Interact.started += handleAction;

            playerInputs.InGame.Enable();
        }
        else
            Debug.LogError($"wrong player input initialization: id={PlayerId}.");

    }

    void handleAction(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Move":
                _rawInputMovement = context.ReadValue<Vector2>();
                animator.SetBool("IsWalking", context.performed);
                break;
            case "Run":
                _isRunning = true; // context.performed;
                break;
            case "Jump":
                if (Grounded) // && context.canceled)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * gravity);
                    animator.SetTrigger("Jump");
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

    public void UnbindInputActions()
    {
        playerInputs.InGame.Move.started -= handleAction;
        playerInputs.InGame.Move.performed -= handleAction;
        playerInputs.InGame.Move.canceled -= handleAction;
        playerInputs.InGame.Run.performed -= handleAction;
        playerInputs.InGame.Jump.canceled -= handleAction;
        playerInputs.InGame.Attack.started -= handleAction;
        playerInputs.InGame.Skill.started -= handleAction;
        playerInputs.InGame.Skill.performed -= handleAction;
        playerInputs.InGame.Skill.canceled -= handleAction;
        playerInputs.InGame.Interact.started -= handleAction;

        playerInputs.InGame.Disable();
    }

    #endregion
}
