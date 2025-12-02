using FS_SwingSystem;
using FS_ThirdPerson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Rope
{
    public class PlayerRopeController : MonoBehaviour
    {
        public SwingRope rope { get; private set; }

        [Header("Rope")]
        [Tooltip("transform for holding the rope")]
        [SerializeField] Transform ropeHoldTransform;
        [Tooltip("hook object to throw")]
        [SerializeField] Transform hookTransform;
        [Tooltip("object to attach the rope")]
        [SerializeField] Transform attachTransform;
        [Tooltip("minimum distance required for the swing")]
        [SerializeField] float minDistance = 2;
        [Tooltip("maximum distance")]
        [SerializeField] float MaxDistance = 10;
        [Tooltip("current rope length")]
        [SerializeField] float ropeLength = 10;
        [SerializeField] float ropeRadius = 0.2f;
        [SerializeField] float ropeWidth = 0.1f;
        [SerializeField] Material ropeMaterial;
        [Tooltip("Duration of the rope throw.")]
        [SerializeField] float throwDuration = 0.5f;
        [SerializeField][Range(0, 1)] float dampening = 0.5f;
        [SerializeField] int ropeResolution = 50;
        [Tooltip("Height of the rope throw.")]
        [SerializeField] float throwHeight = 1;

        [Header("Climb & Swing")]
        [SerializeField] float climbSpeed = 3;
        [SerializeField] float swingForce = 10;
        [SerializeField][Range(0, 1)] float swingRotationSpeed = 1f;
        [SerializeField][Range(0, 90)] float maxSwingAngle = 90;
        [SerializeField] float forwardLandForceMultiplier = 1;
        [SerializeField] float upwardLandForceMultiplier = 2;
        [SerializeField][Range(0, 1)] float damping = 0.5f;
        [SerializeField] float gravity = 9.81f;
        [SerializeField][Range(0, 1)] float collisionFriction = 0.2f;

        PlayerController playerController;
        
        public bool IsThrowing;
        public bool InAction;
        public bool IsClimbing;
        public bool InSwing;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        bool isSwingInitialized = false;

        private void FixedUpdate()
        {
            if (rope == null) return;
            rope.RopeUpdate();

            // update before swing
            pivotPoint = attachTransform.position;

            if (InSwing)
            {
                if (!isSwingInitialized)
                {
                    initializeRopeSwing();
                    isSwingInitialized = true;
                }
                updateRopeSwing();
                handleCollision();
            }
            else
            {
                // fall before swing
                checkSwingStart(out isSwingInitialized);
            }

            // update rope data here
            ropeVector = (ropeHoldTransform.position - attachTransform.position).normalized * ropeLength;

            if (IsClimbing)
            {
                handleClimbing();
            }
        }

        #region Rope Properties

        private float getDistance()
        {
            return Vector3.Distance(attachTransform.position, ropeHoldTransform.position);
        }

        #endregion

        #region Rope Actions

        public void ThrowRope()
        {
            if (rope != null)
            {
                rope.DeleteRope();
                rope = null;
            }

            rope = new SwingRope(ropeRadius, ropeWidth, ropeMaterial, throwDuration, ropeLength, dampening, ropeResolution, throwHeight, transform, ropeHoldTransform, ropeHoldTransform, ropeHoldTransform, attachTransform, hookTransform);
            rope.SetRopeState(RopeState.Throwing);
            rope.StartThrow(ropeHoldTransform, attachTransform.position);
            StartCoroutine(waitForRopeThrow());
        }

        private IEnumerator waitForRopeThrow()
        {
            while (!rope.HasReachedTarget())
                yield return null;
            rope.SetRopeState(RopeState.Normal);
        }

        public bool RetrieveRope()
        {
            if (rope != null)
            {
                rope.DeleteRope();
                return true;
            }
            return false;
        }

        #endregion

        #region Swing

        Vector3 preSwingVelocity;
        Vector3 swingVelocity;
        Vector3 swingDirection;
        Vector3 swingDirectionInput;
        Vector3 ropeVector;
        Vector3 pivotPoint;

        Vector3 initialSwingDirection = Vector3.forward;
        Vector3 initPlayerVelocity;
        Vector3 swingDirectionXZ;
        private float theta;
        private float omega;

        private void checkSwingStart(out bool isSwingInitialized)
        {
            if (
                    getDistance() >= ropeLength  // rope is taut
                    && attachTransform.transform.position.y > ropeHoldTransform.position.y + ropeLength * Mathf.Cos(maxSwingAngle * Mathf.Deg2Rad)  // player must be lower than the attach point
                    && !playerController.Grounded
                )
            {
                InSwing = true;
                isSwingInitialized = false;
            }
            else
            {
                InSwing = false;
                isSwingInitialized = false;
            }

            playerController.rb.useGravity = false;
        }

        private void initializeRopeSwing()
        {
            // animation
            playerController.animator.Play("Idle", 0, 0f);
            // disable player controller
            playerController.enabled = false;
            playerController.rb.WakeUp();
        }

        private void updateRopeSwing()
        {
            if (playerController.Grounded) return;
            float dt = Time.fixedDeltaTime;

            // 1 synchronize player position and rope
            Vector3 playerPos = playerController.transform.position;
            Vector3 dir = (playerPos - pivotPoint).normalized;
            swingDirectionXZ = new Vector3(dir.x, 0, dir.z).normalized;

            theta = Mathf.Atan2(
                Vector3.Dot(dir, swingDirectionXZ),
                -dir.y
            );
            Vector3 tangent =
                swingDirectionXZ * Mathf.Cos(theta) +
                Vector3.down * -Mathf.Sin(theta);
            float tangentialSpeed = Vector3.Dot(playerController.rb.velocity, tangent);
            omega = tangentialSpeed / ropeLength;

            // 2 handle input
            if (swingDirectionInput != Vector3.zero)
            {
                // rotate swing direction
                swingDirectionXZ = Vector3.MoveTowards(
                    swingDirectionXZ,
                    swingDirectionInput,
                    swingRotationSpeed * dt
                ).normalized;

                playerController.playerObj.transform.rotation = Quaternion.LookRotation(new Vector3(swingDirectionXZ.x, 0, swingDirectionXZ.z), Vector3.up);
                //Debug.Log(swingDirectionXZ);

                float angleDiff = Vector3.SignedAngle(swingDirectionXZ, swingDirectionInput, Vector3.up);
                float torque = swingForce * (angleDiff / 180f);
                omega += torque / ropeLength * dt;
            }

            // 3 simulate swing
            // angular acceleration
            float alpha = (gravity / ropeLength) * Mathf.Sin(theta) - damping * omega;
            // angular velocity and angle
            omega += alpha * dt;
            theta += omega * dt;
            // position
            Vector3 offset = swingDirectionXZ * (Mathf.Sin(theta) * ropeLength)
                + Vector3.down * (Mathf.Cos(theta) * ropeLength);
            Vector3 targetPos = pivotPoint + offset;
            // swing velocity
            tangent =
                swingDirectionXZ * (Mathf.Cos(theta)) +
                Vector3.down * -(Mathf.Sin(theta));
            swingVelocity = tangent * (omega * ropeLength);
            // move the player
            Vector3 posError = targetPos - playerController.transform.position;
            Vector3 positionCorrection = posError * 2f; // position correction factor
            playerController.rb.velocity = swingVelocity;
        }

        private void handleCollision()
        {
            float distance = swingVelocity.magnitude * Time.deltaTime;

            if (Physics.SphereCast(transform.position,
                                   playerController.radius,
                                   swingDirection.normalized,
                                   out RaycastHit hit,
                                   distance))
            {
                Vector3 reflected = Vector3.Reflect(swingVelocity, hit.normal);
                reflected += Vector3.up * 0.3f;
                swingVelocity = reflected * collisionFriction;
                Vector3 separation = hit.normal * 0.1f;
                playerController.MoveTo(separation);
                Vector3 slide = Vector3.ProjectOnPlane(reflected, hit.normal);
                swingVelocity = slide * collisionFriction;
            }
        }

        #endregion

        #region Climb

        float climbUp;

        public void StartClimbing()
        {
            playerController.UnbindInputActions();
            this.initialInputAction();
            playerInput.SwitchCurrentActionMap("OnRope");
        }

        private void handleClimbing()
        {
            float climbMovement = 0;

            climbMovement =
                (
                    ropeLength >= minDistance
                    && ropeLength <= MaxDistance
                    && (climbUp == 1 || playerController.Grounded == false)
                ) ?
                climbSpeed * Time.deltaTime * climbUp : 0;

            ropeLength = Mathf.Clamp(ropeLength - climbMovement, minDistance, MaxDistance);
            playerController.MoveTo(pivotPoint + ropeVector);
        }

        #endregion

        #region Landing

        private void exitFromRope()
        {
            this.UnbindInputActions();
            rope.SetRopeState(RopeState.NoPlayer);
            rope.ResetRope();
            rope.DeleteRope();
            rope = null;
            // animation
            
            handleLanding(); 
        }

        private void handleLanding()
        {
            Vector3 direction = swingVelocity.normalized;
            direction.y = 0;

            //playerController.rb.AddForce(direction * swingVelocity.magnitude * forwardLandForceMultiplier + Vector3.up * swingVelocity.y * upwardLandForceMultiplier, ForceMode.Impulse);

            playerController.rb.velocity = direction * swingVelocity.magnitude * forwardLandForceMultiplier + Vector3.up * swingVelocity.y * upwardLandForceMultiplier;

            StartCoroutine(waitForLanding());
        }

        private IEnumerator waitForLanding()
        {
            playerController.rb.useGravity = true;
            while (!playerController.Grounded)
            {
                playerController.GroundedCheck();
                yield return null;
            }

            playerController.enabled = true;
            playerController.BindInputAction();
        }

        #endregion

        #region Inputs

        PlayerInput playerInput;
        PlayerInputs playerInputs;
        float holdTime = 1f;
        bool isHolding;

        public bool HookInputHolding { get; private set; }
        public bool HookReleaseDown { get; private set; }
        public bool RopeClimbModifierHolding { get; private set; }

        void initialInputAction()
        {
            playerInput = playerController.playerInput;
            playerInputs = playerController.playerInputs;

            playerInput.SwitchCurrentActionMap("OnRope");

            playerInputs.OnRope.Release.performed += handleAction;
            playerInputs.OnRope.ClimbUp.performed += handleAction;
            playerInputs.OnRope.ClimbUp.canceled += handleAction;
            playerInputs.OnRope.ClimbDown.performed += handleAction;
            playerInputs.OnRope.ClimbDown.canceled += handleAction;
            playerInputs.OnRope.Swing.performed += handleAction;
            playerInputs.OnRope.Swing.canceled += handleAction;

            playerInputs.OnRope.Enable();
        }

        void handleAction(InputAction.CallbackContext context)
        {
            switch(context.action.name)
            {
                case "Release":
                    if (context.performed)
                    {
                        IsClimbing = false;
                        InSwing = false;
                        exitFromRope();
                    }
                    break;
                case "ClimbUp":
                    if (context.performed && rope != null)
                    {
                        climbUp = 1;
                        IsClimbing = true;
                    }
                    else if (context.canceled)
                    {
                        climbUp = 0;
                        IsClimbing = false;
                    }
                    break;
                case "ClimbDown":
                    if (context.performed && rope != null)
                    {
                        climbUp = -1;
                        IsClimbing = true;
                    }
                    else if (context.canceled)
                    {
                        climbUp = 0;
                        IsClimbing = false;
                    }
                    break;
                case "Swing":
                    if (context.performed && rope != null)
                    {
                        Vector2 swingInput = context.ReadValue<Vector2>();
                        swingDirectionInput = playerController.playerCamera.transform.parent.forward * swingInput.y + playerController.playerCamera.transform.parent.right * swingInput.x;
                    }
                    else if (context.canceled)
                    {
                        swingDirectionInput = Vector3.zero;
                    }
                    break;
                default:
                    break;
            }
        }

        public void UnbindInputActions()
        {
            playerInputs.OnRope.Release.performed -= handleAction;
            playerInputs.OnRope.ClimbUp.started -= handleAction;
            playerInputs.OnRope.ClimbUp.canceled -= handleAction;
            playerInputs.OnRope.ClimbDown.started -= handleAction;
            playerInputs.OnRope.ClimbDown.canceled -= handleAction;
            playerInputs.OnRope.Swing.performed -= handleAction;
            playerInputs.OnRope.Swing.canceled -= handleAction;

            playerInputs.OnRope.Disable();
        }

        #endregion
    }
}
