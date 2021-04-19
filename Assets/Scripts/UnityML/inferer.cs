using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using Random = UnityEngine.Random;

public class inferer : MonoBehaviour, InputHandler
{
    
    public GameObject goal;
    public NNModel modelAsset;
    public float maxDistance = 100 * 1.41f;
    public float mult = 1f;

    
    private Model m_RuntimeModel;
    private IWorker worker;

    void Start()
    {   
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        print(m_RuntimeModel.outputs[0]);
        print(m_RuntimeModel.outputs[1]);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpRef, m_RuntimeModel);
        StartCoroutine(AskForDecision());
    }

    private Vector3 _movement;

    private int counter = 0;
    IEnumerator AskForDecision()
    {
        while (true)
        {
            counter++;
            if (counter == 3)
            {
                Vector3 localNorm = transform.localPosition / maxDistance;
                Vector3 dir = (goal.transform.localPosition - transform.localPosition) / maxDistance;

                float[] obs = {localNorm.x, localNorm.y, localNorm.z, dir.x, dir.y, dir.z};
                Tensor input = new Tensor(1, 6, obs);
                worker.Execute(input);
            
                Tensor output = worker.PeekOutput("21");
                float[] _movementArray = output.ToReadOnlyArray();
                _movement = new Vector3(_movementArray[0], _movementArray[2], _movementArray[1]) * mult;
                print(_movement);
                input.Dispose();
                output.Dispose();
                counter = 0;
            }
            yield return null;
        }
        yield return null;
    }
    private void Update()
    {
        
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
        return false;
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
