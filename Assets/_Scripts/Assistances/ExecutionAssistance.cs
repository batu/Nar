using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionAssistance : MonoBehaviour, IAssistance
{
    private Inferer _inferer;
    public bool alwaysOn = false;

    public bool AlwaysOn
    {
        get => alwaysOn;
        set => alwaysOn = value;
    }
    private void Start()
    {
        _inferer = GetComponent<Inferer>();
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
    { }
}
