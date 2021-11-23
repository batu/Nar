using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InputHandler
{
   
    Vector3 GetMoveInput();
    float GetLookInputsHorizontal();
    float GetLookInputsVertical();

    bool GetJumpInputDown();

    bool GetSprintInputHeld();

    
    bool GetCrouchInputDown();

}
