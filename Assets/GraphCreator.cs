using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphCreator : MonoBehaviour
{
    private void Start()
    {
        CreateGraph.CreateGraphStructure(transform);
    }
}
