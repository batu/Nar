using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = System.Random;

// ReSharper disable once CheckNamespace
public class NavigationAgent :  Agent, InputHandler
{
    public GameObject goal;

    [Header("Observation")]
    public int occupancyGridXZ = 2;
    public int occupancyGridY = 1;

    [HideInInspector]
    public float maxDistance = 100 * 1.41f;


    // Movement
    private PlayerCharacterController _characterController;
    private Vector3 _movement;
    private float jump = 0;

    // Reward
    const float existentialPunishment = -1f / 1024f;
    private float lastDistance;
    private float lastShapeReward = 0;

    public EnvOccupancyGrid occupancyGrid;

    public delegate void EpisodeStarted();
    public EpisodeStarted StartEpisode;

    void Start()
    {
        _characterController = GetComponent<PlayerCharacterController>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        StartEpisode();
        lastDistance = Vector3.Distance(transform.position, goal.transform.position) / maxDistance;
        lastShapeReward = 0;
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
        float zMovement= Mathf.Clamp(actionBuffers.ContinuousActions[1],-1f, 1f);
        jump = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        _movement = new Vector3(xMovement, 0f, zMovement);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var localPosition = transform.localPosition;
        var goalPosition = goal.transform.localPosition;

        sensor.AddObservation(localPosition / maxDistance);
        sensor.AddObservation(goalPosition / maxDistance);

        Vector3 dir = goalPosition - localPosition;
        sensor.AddObservation(dir / maxDistance);
        sensor.AddObservation(dir.normalized);
        sensor.AddObservation(dir.magnitude / maxDistance);
        
        sensor.AddObservation(_characterController.m_remainingJumpCount / 2f);
        sensor.AddObservation(_characterController.characterVelocity / 10f);
        sensor.AddObservation(_characterController.isGrounded);
        
        sensor.AddObservation(occupancyGrid.GetPlayerArea(localPosition, occupancyGridXZ, occupancyGridY, occupancyGridXZ));
        HandleReward();
    }

    private void OnDrawGizmosSelected()
    {
        // occupancyGrid.VisualizePlayerOccupancy(transform.localPosition, occupancyGridXZ, occupancyGridY, occupancyGridXZ);
    }


    void HandleReward()
    {
        float thisDistance = Vector3.Distance(transform.position, goal.transform.position) / maxDistance;
        float shapeReward = lastDistance - thisDistance;
        AddReward(shapeReward);
        AddReward(existentialPunishment);
        AddReward(-lastShapeReward);
        
        lastDistance = thisDistance;
        lastShapeReward = shapeReward;
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
            AddReward(1f);
            EndEpisode();
        }
    }

    public Vector3 GetMoveInput()
    {
        return _movement; // WASD
    }
    
    
    public float GetLookInputsHorizontal()
    {
        return 0; // From mouse, camera direction
    }

    public float GetLookInputsVertical()
    {
        return 0; // From mouse, camera direction
    }

    public bool GetJumpInputDown()
    {
        return jump > .5f;
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
