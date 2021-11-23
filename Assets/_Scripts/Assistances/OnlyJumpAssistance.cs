using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

public class OnlyJumpAssistance : MonoBehaviour, IAssistance
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
        return null;
    }

    public bool? GetJumpInputDown()
    {
        return _inferer.GetJumpInputDown();
    }

    public void StartAssistance()
    { }
}
