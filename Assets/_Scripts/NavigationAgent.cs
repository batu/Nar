using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

// ReSharper disable once CheckNamespace
public class NavigationAgent :  Agent, InputHandler
{
    public GameObject goal;

    [Header("Observations")]
    public bool useAbsolutePositions = true;
    [SerializeField] public bool useDepthMask = true;
    [SerializeField] public bool useOccupancyGrid = true;
    [SerializeField] public bool useLocalRaycasts = true;
    
    [Header("Movement")]
    public bool useRotation = false;
    public float randomizationPercentage = 0f;
    
    private DepthMaskObservation _depthMaskObservation;
    private OccupancyGridObservation _occupancyGridObservation;
    private WhiskerObservation _whiskerObservation;
    private VectorObservation _vectorObservation;
    
    [HideInInspector]
    public float maxDistance = 100 * 1.41f;
    private bool _agentDone = false;
    private bool _success = false;
    
    // Movement
    private PlayerCharacterController _characterController;
    private DecisionRequester _decisionRequester;
    private Vector3 _movement;
    private float _jump = 0;
    private float _rotationX = 0;
    private float _rotationY = 0;

    // Reward
    [Header("Rewards")] [SerializeField]
    public bool useDense = true;
    [SerializeField]
    public bool usePBRS = true;
    
    private float _existentialPunishment = -1f / 100f;
    private float _lastDistance;
    private float _lastShapeReward = 0;

    public delegate void EpisodeStarted();
    public EpisodeStarted StartEpisode;
    [CanBeNull] private SuccessRateMeasure _successRateMeasure;
    [CanBeNull] private DecisionPeriodRandomizer _decisionPeriodRandomizer;

    private float _initMaxSpeedOnGround;
    private float _initMovementSharpnessOnGround;
    private float _initMaxSpeedInAir;
    private float _initAccelerationSpeedInAir;
    private float _initJumpForce;

    private bool isGhost = false;
    private void Awake()
    {
        if (useRotation)
        {
            BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();
            behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(5);
        }
        
        _characterController = GetComponent<PlayerCharacterController>();
        _decisionRequester = GetComponent<DecisionRequester>();
        
        _vectorObservation = GetComponent<VectorObservation>();
        _depthMaskObservation = GetComponent<DepthMaskObservation>();
        _whiskerObservation = GetComponent<WhiskerObservation>();
        _occupancyGridObservation = GetComponent<OccupancyGridObservation>();
        
        _initMaxSpeedOnGround = _characterController.maxSpeedOnGround;
        _initMovementSharpnessOnGround = _characterController.movementSharpnessOnGround;
        _initMaxSpeedInAir = _characterController.maxSpeedInAir;
        _initAccelerationSpeedInAir = _characterController.accelerationSpeedInAir;
        _initJumpForce = _characterController.jumpForce;

        isGhost = transform.name == "Ghost";
    }

    public void RandomizePlayerMovement()
    {
        float randomRange = _initMaxSpeedOnGround * randomizationPercentage;
        float randomAmount = UnityEngine.Random.Range(-randomRange,  randomRange);
        _characterController.maxSpeedOnGround = _initMaxSpeedOnGround + randomAmount;
        
        randomRange = _initMovementSharpnessOnGround * randomizationPercentage;
        randomAmount = UnityEngine.Random.Range(-randomRange,  randomRange);
        _characterController.movementSharpnessOnGround = _initMovementSharpnessOnGround + randomAmount;

        randomRange = _initMaxSpeedInAir * randomizationPercentage;
        randomAmount = UnityEngine.Random.Range(-randomRange,  randomRange);
        _characterController.maxSpeedInAir = _initMaxSpeedInAir + randomAmount;

        randomRange = _initAccelerationSpeedInAir * randomizationPercentage;
        randomAmount = UnityEngine.Random.Range(-randomRange,  randomRange);
        _characterController.accelerationSpeedInAir = _initAccelerationSpeedInAir + randomAmount;

        randomRange = _initJumpForce * randomizationPercentage;
        randomAmount = UnityEngine.Random.Range(-randomRange,  randomRange);
        _characterController.jumpForce = _initJumpForce + randomAmount;
    }

    public void ResetPlayerMovement()
    {
        _characterController.maxSpeedOnGround = _initMaxSpeedOnGround;
        _characterController.movementSharpnessOnGround = _initMovementSharpnessOnGround;
        _characterController.maxSpeedInAir = _initMaxSpeedInAir;
        _characterController.accelerationSpeedInAir = _initAccelerationSpeedInAir;
        _characterController.jumpForce = _initJumpForce;
    }
    
    void Start()
    {
        _existentialPunishment = -1f / ((float)MaxStep / _decisionRequester.DecisionPeriod);
        _successRateMeasure = FindObjectOfType<SuccessRateMeasure>();
        _decisionPeriodRandomizer = FindObjectOfType<DecisionPeriodRandomizer>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            EndEpisode();
        }

        if (!_agentDone && transform.position.y < -15)
        {
            EpisodeFailed();
        }
    }

    public void EpisodeFailed()
    {
        _agentDone = true;
        _success = false;
    }


    public override void OnEpisodeBegin()
    {
        if (randomizationPercentage != 0)
        {   
            RandomizePlayerMovement();
        }
        StartEpisode?.Invoke();
        _success = false;
        _agentDone = false;
        _lastDistance = Vector3.Distance(transform.position, goal.transform.position) / maxDistance;
        _lastShapeReward = 0;
    }
    

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetButton("Jump") ? 1f : 0f;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float xMovement = Mathf.Clamp(actionBuffers.ContinuousActions[0],-1f, 1f);
        float zMovement = Mathf.Clamp(actionBuffers.ContinuousActions[1],-1f, 1f);
        _movement = new Vector3(xMovement, 0f, zMovement);

        _jump = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);

        _rotationX = useRotation ?  Mathf.Clamp(actionBuffers.ContinuousActions[3],-1f, 1f) : 0;
        _rotationY = useRotation ?  Mathf.Clamp(actionBuffers.ContinuousActions[4],-1f, 1f) : 0;

        if (_agentDone)
        {
            _jump = 0;
            _movement = Vector3.zero;
            _rotationX = 0;
            _rotationY = 0;
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_vectorObservation.GetObservation());

        if (useLocalRaycasts)
        {
            sensor.AddObservation(_whiskerObservation.GetObservation());
        }
        
        if (useDepthMask)
        {
            sensor.AddObservation(_depthMaskObservation.GetObservation());
        }
        
        if (useOccupancyGrid)
        {
            sensor.AddObservation(_occupancyGridObservation.GetObservation());
        }

        HandleReward();
    }

  
    private void FixedUpdate()
    {
        UpdateDecisionFrequency();
        
        if (_agentDone && (Academy.Instance.StepCount) % _decisionRequester.DecisionPeriod == 0)
        {
            if (_success)
            {
                AddReward(1f);
                _successRateMeasure?.UpdateResults(SuccessRateMeasure.EpisodeResult.Success);
            }
            else
            {
                AddReward(-1f);
                _successRateMeasure?.UpdateResults(SuccessRateMeasure.EpisodeResult.Failed);
            }
            EndEpisode();
        }

        if (!isGhost && StepCount == MaxStep - 1)
        {
            _successRateMeasure?.UpdateResults(SuccessRateMeasure.EpisodeResult.Timedout);
        }
        
    }

    private void UpdateDecisionFrequency()
    {
        if (_decisionPeriodRandomizer?.randomizeDecisionPeriod ?? false)
        {
            _decisionRequester.DecisionPeriod = _decisionPeriodRandomizer.currentDecisionPeriod;
        }
    }

    void HandleReward()
    {
        float thisDistance = Vector3.Distance(transform.position, goal.transform.position) / maxDistance;
        float shapeReward = _lastDistance - thisDistance;
        AddReward(_existentialPunishment);

        if (useDense)
            AddReward(shapeReward);
    
        if (usePBRS)
            AddReward(-_lastShapeReward * .99f);
        
        _lastDistance = thisDistance;
        _lastShapeReward = shapeReward;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // This is for inference using Barracuda.
        if (hit.transform.CompareTag("Goal") && !this.isActiveAndEnabled)
        {
            StartEpisode();
            return;
        }
        
        if (hit.transform.CompareTag("Goal"))
        {
            _agentDone = true;
            _success = true;
        }
    }



    public Vector3 GetMoveInput()
    {
        return _movement; // WASD
    }
    
    
    public float GetLookInputsHorizontal()
    {
        return _rotationX; // From mouse, camera direction
    }

    public float GetLookInputsVertical()
    {
        return _rotationY; // From mouse, camera direction
    }

    public bool GetJumpInputDown()
    {
        return _jump > .5f;
    }


    public bool GetSprintInputHeld()
    {
        return false; // Shift
    }

    public bool GetCrouchInputDown()
    {
        return false; // Ctrl
    }

  
}
