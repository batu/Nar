using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Barracuda;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class InfererValue : MonoBehaviour
{
    public bool NoVelocity = false;
    public float Height = 10;
    public TextMeshProUGUI valueText;
    public int decisionFrequency = 5;
    public NNModel modelAsset;

    private NavigationAgent _navigationAgent;
    private Model _runtimeModel;
    private IWorker _worker;
    
    private VectorObservation _vectorObservation;
    private DepthMaskObservation _depthMaskObservation;
    private OccupancyGridObservation _occupancyGridObservation;
    private WhiskerObservation _whiskerObservation;

    private float MaxValue = -100;
    private float MinValue = 100;

    private int _inputShape;

    private GridViz _gridViz;

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
        _inputShape = 508; //463;
        print($"Input shape for the model: {_runtimeModel.outputs[0]}");
        // _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpRef, _runtimeModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, _runtimeModel);
        
        StartCoroutine(AskForValue());

        _gridViz = FindObjectOfType<GridViz>();

        //FillValueGrid(_gridViz);

    }


    private void FillValueGrid(GridViz gridViz)
    {
        var cc = FindObjectOfType<PlayerCharacterController>();
        cc.enabled = false;
        Vector3 offset = new Vector3(-41.94f, 0.1f, -43.86f);
        for (int x = 0; x < 130; x++)
        {
            for (int z = 0; z < 130; z++)
            {
                transform.position = new Vector3(x * 7, Height, z * 7) + offset;
                float value = GetValue();
                gridViz.grid.SetValue(x, z, value * 100);
            }
        }
        if (normalizeValues)
            _gridViz.grid.NormalizeValues();
        _gridViz.heatmap.UpdateHeatMapVisual();
        _gridViz.transform.position = new Vector3(-90, Height, -90);
        _gridViz.transform.rotation = Quaternion.Euler(90,0,0);
    }

    public bool normalizeValues = false;
    private Vector3 _movement;
    private int _counter = 0;
    private float _jump;

    IEnumerator AskForValue()
    {
        while (true)
        {
            _counter++;
            if (_counter == decisionFrequency)
            {
                float value = GetValue();

                MaxValue = Mathf.Max(MaxValue, value);
                MinValue = Mathf.Min(MinValue, value);
                valueText.text = $"Value: {value}";

                _gridViz.grid.SetValue(0, 0, value);
                _counter = 0;

            }
            yield return null;
        }
    }

    private float GetValue()
    {
        List<float> obsList = new List<float>();
        obsList.Add(_vectorObservation.GetObservation());

        if (NoVelocity)
        {
            obsList[9] = 0;
            obsList[10] = 0;
            obsList[11] = 0;
        }


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

        // Actions assumed to be stationary. Change it for later.
        obsList.Add(new float[] {0, 0, 0});


        Tensor input = new Tensor(1, _inputShape, obsList.ToArray());
        _worker.Execute(input);
        Tensor output = _worker.PeekOutput();
        float[] valueArray = output.ToReadOnlyArray();
        
        input.Dispose();
        output.Dispose();
        return valueArray[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerCharacterController cc = GetComponent<PlayerCharacterController>();
            cc.characterVelocity = Vector3.zero;
            cc.enabled = false;
            transform.position = new Vector3(15, 5, 3);
            StartCoroutine(EnableCC(cc));

        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            FillValueGrid(_gridViz);
        }
    }

    IEnumerator EnableCC(PlayerCharacterController cc)
    {
        yield return new WaitForSeconds(0.1f);
        cc.enabled = true;
    }
}
