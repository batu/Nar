using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void Add(this List<float> l, Vector3 vec)
    {
        l.Add(vec.x);
        l.Add(vec.y);
        l.Add(vec.z);
    }

    public static void Add(this List<float> l, float f)
    {
        l.Add(f);
    }

    public static void Add(this List<float> l, bool b)
    {
        l.Add(b ? 1 : 0);
    }

    public static void Add(this List<float> l, float[] arr)
    {
        foreach (float t in arr)
        {
            l.Add(t);
        }
    }
}
