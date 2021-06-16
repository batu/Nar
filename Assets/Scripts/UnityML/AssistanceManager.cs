using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;

public class AssistanceManager : MonoBehaviour, InputHandler
{
    private enum  AssistanceTypes {Execution, Planning, OnlyMove, OnlyJump}

    [SerializeField]
    private AssistanceTypes activeAssistance = AssistanceTypes.Execution;
    private Vector3 _movement;
    private bool _jump;

    private PlayerInputHandler _playerInputHandler;
    private IAssistance _assistance;
    private Inferer _inferer;

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _assistance = GetComponent<IAssistance>();
    }

    // Update is called once per frame
    void Update()
    {
        _movement = _playerInputHandler.GetMoveInput();
        _jump = _playerInputHandler.GetJumpInputDown();

        if (StartAssisting())
        {
            _assistance.StartAssistance();
        }

        text.color = Color.white;
        if (KeepAssisting() || _assistance.AlwaysOn)
        {
            text.color = Color.green;
            bool? possibleJump = _assistance.GetJumpInputDown();
            if (possibleJump != null)
                _jump = (bool) possibleJump;

            Vector3? possibleMovement = _assistance.GetMoveInput();
            if (possibleMovement != null)
                _movement = (Vector3) possibleMovement;
        }

        ChangeAssistanceViaKeyboard();

        if (!IsCorrectAssistance())
        {
            switch (activeAssistance)
            {
                case AssistanceTypes.Execution:
                    _assistance = GetComponent<ExecutionAssistance>();
                    text.text = "Assistance Type: Execution (The Agent moves the player to the Goal)";
                    text.color = Color.white;

                    break;
                case AssistanceTypes.Planning:
                    _assistance = GetComponent<PlanningAssistance>();
                    text.text = "Assistance Type: Planning (The Agent shows the way towards the Goal)";
                    text.color = Color.white;
                    break;
                case AssistanceTypes.OnlyMove:
                    _assistance = GetComponent<OnlyMoveAssistance>();
                    text.text = "Assistance Type: Only Move (The Agent handles moving, player only has to jump)";
                    text.color = Color.green;

                    break;
                case AssistanceTypes.OnlyJump:
                    text.text = "Assistance Type: Only Jump (The Agent handles jumping, player only has to move)";
                    text.color = Color.green;

                    _assistance = GetComponent<OnlyJumpAssistance>();
                    break;
            }
        }

    }

    private void ChangeAssistanceViaKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeAssistance = AssistanceTypes.Execution;
        if (Input.GetKeyDown(KeyCode.Alpha2)) activeAssistance = AssistanceTypes.Planning;
        if (Input.GetKeyDown(KeyCode.Alpha3)) activeAssistance = AssistanceTypes.OnlyJump;
        if (Input.GetKeyDown(KeyCode.Alpha4)) activeAssistance = AssistanceTypes.OnlyMove;
    }

    bool IsCorrectAssistance()
    {
        var type = _assistance.GetType();
        if (type == typeof(ExecutionAssistance) && activeAssistance == AssistanceTypes.Execution)
            return true;
        if (type == typeof(PlanningAssistance) && activeAssistance == AssistanceTypes.Planning)
            return true;
        if (type == typeof(OnlyJumpAssistance) && activeAssistance == AssistanceTypes.OnlyJump)
            return true;
        if (type == typeof(OnlyMoveAssistance) && activeAssistance == AssistanceTypes.OnlyMove)
            return true;
        return false;
    }
    
    private bool StartAssisting()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    private bool KeepAssisting()
    {
        return Input.GetKey(KeyCode.E);
    }

    public Vector3 AssistMove()
    {
        return  Vector3.zero;
    }
    
    public Vector3 GetMoveInput()
    {
        return _movement;
    }

    public bool GetJumpInputDown()
    {
        return _jump;
    }

    public float GetLookInputsHorizontal()
    {
        return _playerInputHandler.GetLookInputsHorizontal();
    }

    public float GetLookInputsVertical()
    {
        return _playerInputHandler.GetLookInputsVertical();

    }

    public bool GetSprintInputHeld()
    {
        return false;
    }

    public bool GetCrouchInputDown()
    {
        return false;
    }
}
