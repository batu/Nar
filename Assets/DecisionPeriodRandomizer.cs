using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class DecisionPeriodRandomizer : MonoBehaviour
{
    public bool randomizeDecisionPeriod = false;
    public int decisionPeriod = 10;
    public int decisionPeriodLowerBound = 5;

    public int currentDecisionPeriod;
    // Start is called before the first frame update
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    void Start()
    {
        currentDecisionPeriod = decisionPeriod;
        if (randomizeDecisionPeriod)
        {
            StartCoroutine(UpdateDecisionPeriod());
        }
    }

    private IEnumerator UpdateDecisionPeriod()
    {
        while (true)
        {
            if (Academy.Instance.StepCount % currentDecisionPeriod == 0)
            {
                currentDecisionPeriod += Random.Range(decisionPeriodLowerBound, decisionPeriod + 1);
            }
            yield return _waitForFixedUpdate;
        }
    }
}
