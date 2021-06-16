using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlanningAssistance : MonoBehaviour, InputHandler, IAssistance
{
    private Inferer _inferer;
    public GameObject Ghost;
    public Transform Goal;
    
    bool _alwaysOn = false;
    public bool AlwaysOn
    {
        get => _alwaysOn;
        set => _alwaysOn = value;
    }

    private PlayerCharacterController _ghostController;
    private bool _started = false;
    private TrailRenderer _tr;


    private void Start()
    {
        _ghostController = Ghost.GetComponent<PlayerCharacterController>();
        _ghostController.m_InputHandler = this;
        _inferer = _ghostController.GetComponent<Inferer>();
        _tr = Ghost.GetComponentInChildren<TrailRenderer>();
    }

    private void Update()
    {
        if (Vector3.Distance(Goal.position, Ghost.transform.position) < 5f)
        {
            _started = false;
        };
    }

    Vector3 InputHandler.GetMoveInput()
    {
        return _started ? _inferer.GetMoveInput() : Vector3.zero;
    }
    
    bool InputHandler.GetJumpInputDown()
    {
        return _started && _inferer.GetJumpInputDown();
    }
    public Vector3? GetMoveInput()
    {
        return null;
    }

    public void StartAssistance()
    {
        _ghostController.characterVelocity = Vector3.zero;
        Ghost.transform.position = transform.position;
        _tr.Clear();
        _started = true;

    }
    public float GetLookInputsHorizontal()
    {
        return 0;
    }

    public float GetLookInputsVertical()
    {
        return 0;
    }

    public bool GetSprintInputHeld()
    {
        return false;
    }

    public bool GetCrouchInputDown()
    {
        return false;
    }

 

    public bool? GetJumpInputDown()
    {
        return null;
    }
}