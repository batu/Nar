using System;
using Unity.Collections.LowLevel.Unsafe;
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
    
    [Header("Actions")]
    public bool useRotation = false;
    
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
    bool useDense = true;
    [SerializeField]
    bool usePBRS = true;
    
    private float _existentialPunishment = -1f / 100f;
    private float _lastDistance;
    private float _lastShapeReward = 0;

    public delegate void EpisodeStarted();
    public EpisodeStarted StartEpisode;

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
    }

    void Start()
    { 

        
        _existentialPunishment = -1f / ((float)MaxStep / _decisionRequester.DecisionPeriod);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            EndEpisode();
        }

        if (!_agentDone && transform.position.y < -15)
        {
            _agentDone = true;
            _success = false;
        }
    }
    

    public override void OnEpisodeBegin()
    {
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
    
    // Start is called before the first frame update
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
        if (_agentDone && Academy.Instance.StepCount % _decisionRequester.DecisionPeriod == 0)
        {
            if (_success)
            {
                AddReward(1f);
            }
            else
            {
                AddReward(-1f);
            }
            EndEpisode();
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
        return true; // Shift
    }

    public bool GetCrouchInputDown()
    {
        return false; // Ctrl
    }

  
}
