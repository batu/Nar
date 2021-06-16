using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Barracuda;
using UnityEngine;
using Random = UnityEngine.Random;

public class Inferer : MonoBehaviour, InputHandler
{

    public int decisionFrequency = 5;
    public NNModel modelAsset;

    private NavigationAgent _navigationAgent;
    private Model _runtimeModel;
    private IWorker _worker;
    
    private VectorObservation _vectorObservation;
    private DepthMaskObservation _depthMaskObservation;
    private OccupancyGridObservation _occupancyGridObservation;
    private WhiskerObservation _whiskerObservation;

    private int _inputShape;

    void Start()
    {
        Debug.LogWarning("The Quaternion turn adjustment is active!");
        _navigationAgent = GetComponent<NavigationAgent>();
        
        _vectorObservation = GetComponent<VectorObservation>();
        _depthMaskObservation = GetComponent<DepthMaskObservation>();
        _whiskerObservation = GetComponent<WhiskerObservation>();
        _occupancyGridObservation = GetComponent<OccupancyGridObservation>();
        
        
        _runtimeModel = ModelLoader.Load(modelAsset);
        _inputShape = _runtimeModel.inputs[0].shape[_runtimeModel.inputs[0].shape.Length - 1];
        _inputShape = 505; //463;
        print($"Input shape for the model: {_runtimeModel.outputs[0]}");
        // _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpRef, _runtimeModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, _runtimeModel);
        
        StartCoroutine(AskForDecision());

    }

    private Vector3 _movement;
    private int _counter = 0;
    private float _jump;

    IEnumerator AskForDecision()
    {
        while (true)
        {
            _counter++;
            if (_counter == decisionFrequency)
            {
                List<float> obsList = new List<float>();
                obsList.Add(_vectorObservation.GetObservation());
        
                if (_navigationAgent.useLocalRaycasts)
                {
                    obsList.Add(_whiskerObservation.GetObservation());
                }
                
                if (_navigationAgent.useDepthMask)
                {
                    obsList.Add(_depthMaskObservation.GetObservation());
                }
                
                if (_navigationAgent.useOccupancyGrid)
                {
                    obsList.Add(_occupancyGridObservation.GetObservation());
                }

                Tensor input = new Tensor(1, _inputShape, obsList.ToArray());
                _worker.Execute(input);
                Tensor output = _worker.PeekOutput();
                float[] movementArray = output.ToReadOnlyArray();
                
                
                
                _movement = new Vector3(movementArray[0], 0, movementArray[1]);
                _movement =  Quaternion.AngleAxis(-transform.rotation.eulerAngles.y + 90, Vector3.up) * _movement;
                _jump = movementArray[2];
                input.Dispose();
                output.Dispose();
                _counter = 0;
            }
            yield return null;
        }
    }

    public Vector3 GetMoveInput()
    {
        return _movement;
    }

    public float GetLookInputsHorizontal()
    {
        return 0;
    }

    public float GetLookInputsVertical()
    {
        return 0;
    }

    public bool GetJumpInputDown()
    {
        return _jump > .5f;
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
