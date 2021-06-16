using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TestUtils
{
    public static void PrintHello()
    {
        Debug.Log("Hello");
    }

    public static Vector3 newBounceDirection(this Vector3 thisVector3, Vector3 otherVector)
    {
        // do stuff;
        return Vector3.down;
    }
}
