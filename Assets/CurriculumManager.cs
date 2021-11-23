using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class CurriculumManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool Testing { get; set; }

    public float curriculumIncreaseThreshold = .8f;
    public float curriculumIncreaseAmount = 5f;

    private readonly Queue<int> _successes = new Queue<int>();
    public bool curriculumActive;

    public float currentCurriculumDistance = 5f;
    private EnvironmentParameters _envParameters;

    void Start()
    {
        _envParameters = Academy.Instance.EnvironmentParameters;
    }

    public void AddEpisodeResult(bool successful)
    {
        Testing = 0 < _envParameters.GetWithDefault("testing", 0);

        _successes.Enqueue(successful ? 1 : 0);
        while (_successes.Count > 250)
        {
            _successes.Dequeue();
        }

        CheckAndUpdateCurriculum();
    }


    private void CheckAndUpdateCurriculum()
    {
        if (_successes.Count < 250) return;
        
        float currentSuccessRate = (float) _successes.Average();
        if (currentSuccessRate > curriculumIncreaseThreshold)
        {
            currentCurriculumDistance += curriculumIncreaseAmount;

            // Reduce half of it so there is some time for new experiences to be collected in the new threshold.
            for (int i = 0; i < 125; i++)
            {
                _successes.Dequeue();
            }
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
