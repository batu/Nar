using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentConfiguration : MonoBehaviour
{

    public GameObject Environment;

    private EpisodeHandler _episodeHandler;
    private EnvironmentParameters _envParameters;

    public  void Configure()
    {
        _envParameters = Academy.Instance.EnvironmentParameters;
        _episodeHandler = Environment.GetComponent<EpisodeHandler>();
        
        UpdateCurriculum();
        UpdateEnvCount();
    }
    
    private void UpdateEnvCount()
    {
        int numEnvs = (int) _envParameters.GetWithDefault("env_count", 32);
        for (int i = 1; i < numEnvs; i++)
        {
            float seperationDistance = 250f;
            float xPos = i % 5 * seperationDistance;
            float zPos = i / 5 * seperationDistance;
            Vector3 spawnPosition = new Vector3(xPos, 0, zPos);
            Instantiate(Environment, spawnPosition, Quaternion.identity);
        }
    }

    private void UpdateCurriculum()
    {
        _episodeHandler.curriculumEndStep = _envParameters.GetWithDefault("curriculum_length", 10000000);
    }

}
