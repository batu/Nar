using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyMoveAssistance : MonoBehaviour, IAssistance
{
    private Inferer _inferer;
    bool _alwaysOn = true;

    public bool AlwaysOn
    {
        get => _alwaysOn;
        set => _alwaysOn = value;
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
        return null;
    }

    public void StartAssistance() { }
}
