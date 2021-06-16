using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssistance
{
    Vector3? GetMoveInput();
    bool? GetJumpInputDown();

    void StartAssistance();
    bool AlwaysOn { get; }
}
