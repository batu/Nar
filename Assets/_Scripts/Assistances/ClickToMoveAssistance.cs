using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ClickToMoveAssistance : MonoBehaviour, IAssistance
{
    private Inferer _inferer;
    bool _alwaysOn = false;

    private Camera _mainCam;
    public GameObject clickToMoveGoal;
    public GameObject originalGoal;
    private NavigationAgent _navigationAgent;
    private VectorObservation _vectorObservation;

    public bool AlwaysOn
    {
        get => _alwaysOn;
        set => _alwaysOn = value;
    }
    private void Start()
    {
        _mainCam = Camera.main;
        _inferer = GetComponent<Inferer>();
        originalGoal = GameObject.FindWithTag("Goal");
        _navigationAgent = GetComponent<NavigationAgent>();
        _vectorObservation = GetComponent<VectorObservation>();
    }

    public Vector3? GetMoveInput()
    {
        return _inferer.GetMoveInput();
    }

    public bool? GetJumpInputDown()
    {
        return _inferer.GetJumpInputDown();
    }

    public void StartAssistance()
    {
        Vector3 hitPoint = Vector3.zero;
        if (GetClickedPoint(ref hitPoint))
        {
            _vectorObservation.goal = clickToMoveGoal;
            clickToMoveGoal.transform.position = hitPoint;
            AlwaysOn = true;
        };
    }

    private void Update()
    {
        
        if (Vector3.Distance(clickToMoveGoal.transform.position, transform.position) < 2.5f)
        {
            AlwaysOn = false;
        };
    }
    bool GetClickedPoint(ref Vector3 hitPoint)
    {
        RaycastHit hit;
        Ray ray = new Ray(_mainCam.transform.position, _mainCam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
        if (Physics.Raycast(ray, out hit, 100f))
        {
            hitPoint = hit.point;
            return true;
        };
        return false;
    }
}
