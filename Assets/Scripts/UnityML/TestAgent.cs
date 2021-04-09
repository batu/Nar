using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = System.Random;

// ReSharper disable once CheckNamespace
public class TestAgent :  Agent, InputHandler
{
    private Vector3 _movement;
    public GameObject goal;
    private float lastDistance;

    public delegate void EpisodeStarted();
    public EpisodeStarted StartEpisode;
 
    [HideInInspector]
    public float maxDistance = 100 * 1.41f;
    const float existentialPunishment = -1f / 1024f;

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
    }
    

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetButtonDown("Jump") ? 1f : 0f;
    }
    
    // Start is called before the first frame update
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float xMovement = Mathf.Clamp(actionBuffers.ContinuousActions[0],-1f, 1f);
        float jump = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        float zMovement= Mathf.Clamp(actionBuffers.ContinuousActions[1],-1f, 1f);
        _movement = new Vector3(xMovement, jump, zMovement);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var localPosition = transform.localPosition;
        sensor.AddObservation(localPosition / maxDistance);
        sensor.AddObservation(goal.transform.localPosition / maxDistance);
        // sensor.AddObservation((goal.transform.localPosition - localPosition) / maxDistance);
        HandleReward();
    }

    void HandleReward()
    {
        float thisDistance = Vector3.Distance(transform.position, goal.transform.position) / maxDistance;
        AddReward(lastDistance - thisDistance);
        AddReward(existentialPunishment);
        lastDistance = thisDistance;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
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
        return false; //_movement.y > .5f;
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
