﻿using System;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController), typeof(InputHandler), typeof(AudioSource))]
public class PlayerCharacterController : MonoBehaviour
{

    public int publiclen = 1;
    [Header("References")]
    [Tooltip("Reference to the main camera used for the player")]
    public Camera playerCamera;
    [Tooltip("Audio source for footsteps, jump, etc...")]
    public AudioSource audioSource;

    [Header("General")]
    [Tooltip("Force applied downward when in the air")]
    public float gravityDownForce = 60f;
    [Tooltip("Physic layers checked to consider the player grounded")]
    public LayerMask groundCheckLayers = -1;
    [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
    public float groundCheckDistance = 0.05f;

    [Header("Movement")]
    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float maxSpeedOnGround = 15f;
    [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
    public float movementSharpnessOnGround = 15;
    [Tooltip("Max movement speed when crouching")]
    [Range(0,1)]
    public float maxSpeedCrouchedRatio = 0.5f;
    [Tooltip("Max movement speed when not grounded")]
    public float maxSpeedInAir = 15f;
    [Tooltip("Acceleration speed when in the air")]
    public float accelerationSpeedInAir = 25f;
    [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
    public float sprintSpeedModifier = 2f;
    [Tooltip("Height at which the player dies instantly when falling off the map")]
    public float killHeight = -50f;

    [Header("Rotation")]
    [Tooltip("Rotation speed for moving the camera")]
    public float rotationSpeed = 200f;
    [Range(0.1f, 1f)]
    [Tooltip("Rotation speed multiplier when aiming")]
    public float aimingRotationMultiplier = 0.4f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    public float fallSpeedMultiplier = 1f;
    public float jumpForce = 24f;
    public int m_maxAirJumpCount = 1;
    internal int m_remainingJumpCount;

    [Header("Stance")]
    [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
    public float cameraHeightRatio = 0.9f;
    [Tooltip("Height of character when standing")]
    public float capsuleHeightStanding = 1.8f;
    [Tooltip("Height of character when crouching")]
    public float capsuleHeightCrouching = 0.9f;
    [Tooltip("Speed of crouching transitions")]
    public float crouchingSharpness = 10f;

    [Header("Audio")]
    [Tooltip("Amount of footstep sounds played when moving one meter")]
    public float footstepSFXFrequency = 1f;
    [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
    public float footstepSFXFrequencyWhileSprinting = 1f;
    [Tooltip("Sound played for footsteps")]
    public AudioClip footstepSFX;
    [Tooltip("Sound played when jumping")]
    public AudioClip jumpSFX;
    [Tooltip("Sound played when landing")]
    public AudioClip landSFX;
    [Tooltip("Sound played when taking damage froma fall")]
    public AudioClip fallDamageSFX;

    [Header("Fall Damage")]
    [Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
    public bool recievesFallDamage;
    [Tooltip("Minimun fall speed for recieving fall damage")]
    public float minSpeedForFallDamage = 10f;
    [Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
    public float maxSpeedForFallDamage = 30f;
    [Tooltip("Damage recieved when falling at the mimimum speed")]
    public float fallDamageAtMinSpeed = 10f;
    [Tooltip("Damage recieved when falling at the maximum speed")]
    public float fallDamageAtMaxSpeed = 50f;

    public UnityAction<bool> onStanceChanged;

    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isDead { get; private set; }
    public bool isCrouching { get; private set; }
    public float RotationMultiplier = 1;

    Health m_Health;
    internal InputHandler m_InputHandler;
    CharacterController m_Controller;
    PlayerWeaponsManager m_WeaponsManager;
    Actor m_Actor;
    Vector3 m_GroundNormal;
    Vector3 m_CharacterVelocity;
    Vector3 m_LatestImpactSpeed;
    float m_LastTimeJumped = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_footstepDistanceCounter;
    float m_TargetCharacterHeight;

    const float k_JumpGroundingPreventionTime = 0.2f;
    const float k_GroundCheckDistanceInAir = 0.07f;

    WallRun wallRunComponent;
    private int jumpOnceNFrames;
    public int jumpCooldown;
    void Start()
    { 
        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller, this, gameObject);

        DecisionRequester dr = GetComponent<DecisionRequester>();
        jumpOnceNFrames = dr != null ? dr.DecisionPeriod: 10;
        jumpCooldown = jumpOnceNFrames;
        
        m_InputHandler = GetComponent<InputHandler>();
        if (m_InputHandler == null)
        {
            Debug.LogError("Error: Component of type " + typeof(PlayerCharacterController) + " on GameObject " + gameObject.name +
                           " expected to find a component of type " + typeof(InputHandler) + " on GameObject " + gameObject.name + ", but none were found.");
        }

        m_Health = GetComponent<Health>();
        wallRunComponent = GetComponent<WallRun>();

        m_Controller.enableOverlapRecovery = true;
        m_remainingJumpCount = m_maxAirJumpCount;

        if (m_Health != null)
            m_Health.onDie += OnDie;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);
    }

    private void FixedUpdate()
    {
        jumpCooldown--;
        jumpCooldown = Mathf.Max(jumpCooldown, 0);
    }

    void Update()
    {
        // check for Y kill
        if(!isDead && transform.position.y < killHeight)
        {
            if (m_Health != null)
                m_Health.Kill();
        }

        hasJumpedThisFrame = false;

        bool wasGrounded = isGrounded;
        GroundCheck();

        // landing
        HandleLanding(wasGrounded);
        

        // crouching
        if (m_InputHandler.GetCrouchInputDown())
        {
            SetCrouchingState(!isCrouching, false);
        }

        UpdateCharacterHeight(false);
        HandleCharacterMovement();
    }
    
    private void HandleLanding(bool wasGrounded)
    {
        if (isGrounded && !wasGrounded)
        {
            // Fall damage
            float fallSpeed = -Mathf.Min(characterVelocity.y, m_LatestImpactSpeed.y);
            float fallSpeedRatio = (fallSpeed - minSpeedForFallDamage) / (maxSpeedForFallDamage - minSpeedForFallDamage);
            if (recievesFallDamage && fallSpeedRatio > 0f)
            {
                float dmgFromFall = Mathf.Lerp(fallDamageAtMinSpeed, fallDamageAtMaxSpeed, fallSpeedRatio);

                // fall damage SFX
                // audioSource.PlayOneShot(fallDamageSFX);
            }
            else
            {
                // land SFX
                // audioSource.PlayOneShot(landSFX);
            }
        }
    }

    void OnDie()
    {
        isDead = true;

        // Tell the weapons manager to switch to a non-existing weapon in order to lower the weapon
        m_WeaponsManager.SwitchToWeaponIndex(-1, true);
    }

    void GroundCheck()
    {
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;
                    m_remainingJumpCount = m_maxAirJumpCount;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    void HandleCharacterMovement()
    {
        // horizontal character rotation
        // rotate the transform with the input speed around its local Y axis
        transform.Rotate(
            new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * rotationSpeed * RotationMultiplier), 0f),
            Space.Self);

        // vertical camera rotation
        // add vertical inputs to the camera's vertical angle
        m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * rotationSpeed * RotationMultiplier;

        // limit the camera's vertical angle to min/max
        m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

        // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
        if (wallRunComponent != null)
        {
            playerCamera.transform.localEulerAngles =
                new Vector3(m_CameraVerticalAngle, 0, wallRunComponent.GetCameraRoll());
        }
        else
        {
            playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
        }

        // character movement handling
        bool isSprinting = m_InputHandler.GetSprintInputHeld();
        if (isSprinting)
        {
            isSprinting = SetCrouchingState(false, false);
        }

        float speedModifier = isSprinting ? sprintSpeedModifier : 1f;

        // converts move input to a worldspace vector based on our character's transform orientation
        Vector3 moveInput =  m_InputHandler.GetMoveInput();
        if (Mathf.Abs(moveInput.x) > 1 || Mathf.Abs(moveInput.z) > 1)
        {
            // Debug.LogError("MoveInput is received higher than 1!");
        }
        Vector3 clippedMoveInput = new Vector3(Mathf.Clamp(moveInput.x, -1f, 1f),
                                               Mathf.Clamp(moveInput.y, -1f, 1f),
                                               Mathf.Clamp(moveInput.z, -1f, 1f));
        Vector3 worldspaceMoveInput = transform.TransformVector(clippedMoveInput);


        // jumping
        HandleJump();
        
        // handle grounded movement
        if (isGrounded || (wallRunComponent != null && wallRunComponent.IsWallRunning()))
        {
            if (isGrounded)
            {
                // calculate the desired velocity from inputs, max speed, and current slope
                Vector3 targetVelocity = worldspaceMoveInput * (maxSpeedOnGround * speedModifier);
                // reduce speed if crouching by crouch speed ratio
                if (isCrouching)
                    targetVelocity *= maxSpeedCrouchedRatio;
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) *
                                 targetVelocity.magnitude;

                // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity,
                    movementSharpnessOnGround * Time.deltaTime);
            }
            
            // footsteps sound
            float chosenFootstepSFXFrequency =
                (isSprinting ? footstepSFXFrequencyWhileSprinting : footstepSFXFrequency);
            if (m_footstepDistanceCounter >= 1f / chosenFootstepSFXFrequency)
            {
                m_footstepDistanceCounter = 0f;
                //audioSource.PlayOneShot(footstepSFX);
            }

            // keep track of distance traveled for footsteps sound
            m_footstepDistanceCounter += characterVelocity.magnitude * Time.deltaTime;
        }
        // handle air movement
        else
        {
            if (wallRunComponent == null || (wallRunComponent != null && !wallRunComponent.IsWallRunning()))
            {
                // add air acceleration
                characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                // limit air speed to a maximum, but only horizontally
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                float gravityMultiplier = verticalVelocity < 0 ? fallSpeedMultiplier : 1;
                // apply the gravity to the velocity
                characterVelocity += Vector3.down * gravityDownForce * gravityMultiplier * Time.deltaTime;
            }
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        // print($"Char velocity: {characterVelocity}");
        // print($"Char velocity * timeDelta: {characterVelocity * Time.deltaTime}");
        m_Controller.Move(characterVelocity * Time.deltaTime);

        // detect obstructions to adjust velocity accordingly
        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
        {
            // We remember the last impact speed because the fall damage logic might need it
            m_LatestImpactSpeed = characterVelocity;

            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
        }
    }


    private void HandleJump()
    {
        // Get jumps back if wall running.
        m_remainingJumpCount = wallRunComponent.IsWallRunning() ? m_maxAirJumpCount : m_remainingJumpCount;
        
        if ((m_remainingJumpCount > 0 || (wallRunComponent != null && wallRunComponent.IsWallRunning())) &&
            m_InputHandler.GetJumpInputDown() && jumpCooldown == 0)
        {
            jumpCooldown = jumpOnceNFrames;
            
            // If you are in the ground and jumping, you get a "free" ground jump before you start using your "air" jumps
            if (isGrounded) m_remainingJumpCount++;
            
            // force the crouch state to false
            if (SetCrouchingState(false, false))
            {
                if (wallRunComponent.IsWallRunning())
                {
                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                    // then, add the jumpSpeed value upwards
                    characterVelocity += wallRunComponent.GetWallJumpDirection() * jumpForce;
                }
                else if (m_remainingJumpCount > 0)
                {
                    // start by canceling out the vertical component of our velocity
                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                    // then, add the jumpSpeed value upwards
                    characterVelocity += Vector3.up * jumpForce;
                }

                // play sound
                // audioSource.PlayOneShot(jumpSFX);

                // remember last time we jumped because we need to prevent snapping to ground for a short time
                m_LastTimeJumped = Time.time;
                hasJumpedThisFrame = true;
                m_remainingJumpCount--;

                // Force grounding to false
                isGrounded = false;
                m_GroundNormal = Vector3.up;
            }
        }
    }

    // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    // Gets a reoriented direction that is tangent to a given slope
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void UpdateCharacterHeight(bool force)
    {
        // Update height instantly
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * cameraHeightRatio;
        }
        // Update smooth height
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            // resize the capsule and adjust camera position
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, crouchingSharpness * Time.deltaTime);
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
        }
    }

    // returns false if there was an obstruction
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // set appropriate heights
        if (crouched)
        {
            m_TargetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            // Detect obstructions
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(capsuleHeightStanding),
                    m_Controller.radius,
                    -1,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = capsuleHeightStanding;
        }

        if (onStanceChanged != null)
        {
            onStanceChanged.Invoke(crouched);
        }

        isCrouching = crouched;
        return true;
    }
}
