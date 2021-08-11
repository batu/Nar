using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EpisodeHandler : MonoBehaviour
{

    public Transform Agent;
    public Transform Goal;
    public Transform Ground;


    private float maxDistance { get; set; } = 1f;

    [HideInInspector]
    public float xLen;
    [HideInInspector]
    public float zLen;
    public float safetyOffset = 2.5f;

    Vector3 _agentInitialPosition;
    Vector3 _goalInitialPosition;

    public LayerMask spawnableLayers = 1; // Default Layer


    private readonly Collider[] _dummyCollider = new Collider[0];
    private TrailRenderer _trailRenderer;

    public float curriculumEndStep = -1;

    private float _startCurriculumDistance = 10f;
    private float _maxCurriculumDistance = 100f;
    
    private EnvironmentParameters _envParameters;
    float _numEnvsAdjustment;



    // Start is called before the first frame update
    private void Awake()
    {
        NavigationAgent agentScript = Agent.GetComponent<NavigationAgent>();
        agentScript.StartEpisode += RestartEpisode;
        
        xLen = Ground.GetComponent<BoxCollider>().bounds.size.x;
        zLen = Ground.GetComponent<BoxCollider>().bounds.size.z;
        maxDistance = Mathf.Sqrt(xLen * xLen + zLen * zLen);
        agentScript.maxDistance = maxDistance;
        _maxCurriculumDistance = maxDistance;
        
        _agentInitialPosition = Agent.position;
        _goalInitialPosition = Goal.position;
        _trailRenderer = Agent.GetComponentInChildren<TrailRenderer>();
        
        _envParameters = Academy.Instance.EnvironmentParameters;
        _numEnvsAdjustment = _envParameters.GetWithDefault("env_count", 32) / 10f;

    }

    private void Start()
    {
    }
    

    public void RestartEpisode()
    {
        _testing = 0 < _envParameters.GetWithDefault("testing", 0);
        
        MoveAgentRandomly();
        
        bool curriculumActive = Academy.Instance.TotalStepCount * _numEnvsAdjustment < curriculumEndStep;
        if (!curriculumActive || _testing)
        {
            MoveGoalRandomly();
        }
        else
        {
            MoveGoalClose();
        }
        Debug.LogError($"Currently testing: {_testing} at step {Academy.Instance.TotalStepCount * _numEnvsAdjustment}");
        
        // MoveAgenttoInitialPlace();
        // MoveGoaltoInitialPlace();
        _trailRenderer.Clear();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void MoveGoalClose()
    {
        float t = Academy.Instance.TotalStepCount * _numEnvsAdjustment/ curriculumEndStep;
        float curriculumSpawnDistance = Mathf.Lerp(_startCurriculumDistance, _maxCurriculumDistance, t);
        Vector3 raycastHitPos;
        int breaker = 0;
        do
        {
             Vector3 spawnOffset = Random.onUnitSphere * curriculumSpawnDistance;
             spawnOffset.y = Random.Range(0, curriculumSpawnDistance);
             Vector3 raycastPos = spawnOffset + Agent.transform.position;
             Physics.Raycast(origin: raycastPos, direction: Vector3.down, hitInfo: out RaycastHit hit, maxDistance: 250, spawnableLayers.value);
             raycastHitPos = hit.point;
             breaker++;
             if (breaker > 50)
             {
                 MoveAgentRandomly();
             }
             if (breaker > 100)
             {
                 print("BROKEN");
                 Debug.LogException(new Exception("The spawning went into an infinite loop."));
                 raycastHitPos = _goalInitialPosition;
                 break;
             }
        } while (!IsPointWithinBounds(raycastHitPos) || !IsSpawnPointFree(raycastHitPos));
        Goal.position = raycastHitPos  + new Vector3(0, 2, 0);
    }
    void MoveGoalRandomly()
    {
        Vector3 raycastHitPos;
        do {
            raycastHitPos = SampleRandomSpawnPoint();
            _freeBreaker++;
            if (_freeBreaker > 100)
            {
                print("BROKEN");
                break;
            }
        } while (!IsSpawnPointFree(raycastHitPos));
        _freeBreaker = 0;
        Goal.position = raycastHitPos  + new Vector3(0, 2, 0);
    }
    
    void MoveAgentRandomly()
    {
        Vector3 raycastHitPos;
        do {
            raycastHitPos = SampleRandomSpawnPoint();
            _freeBreaker++;
            if (_freeBreaker > 100)
            {
                print("BROKEN");
                break;
            }
        } while (!IsSpawnPointFree(raycastHitPos));
        _freeBreaker = 0;
        Agent.position = raycastHitPos + new Vector3(0, 1, 0);
        _trailRenderer.Clear();
    }

    void MoveAgenttoInitialPlace()
    {
        Agent.position = _agentInitialPosition;
    }

    void MoveGoaltoInitialPlace()
    {
        Goal.position = _goalInitialPosition;

    }

    private void OnDrawGizmos()
    {

            // Vector3 center;
            // float newXPos = (-xLen / 2 + safetyOffset + xLen / 2 - safetyOffset);
            // float newYPos = (100f + 10f) /2f;
            // float newZPos = -zLen / 2 + safetyOffset + zLen / 2 - safetyOffset;
            // center = new Vector3(newXPos, newYPos, newZPos);
            // Gizmos.color = new Color(0, .5f, 0, 0.5f);
            // Gizmos.DrawCube(center, new Vector3((xLen / 2f - safetyOffset) * 2f, 45f, (zLen / 2f - safetyOffset) * 2));
    }

    private Vector3 SampleRandomSpawnPoint()
    {
        Vector3 raycastPos;
        float newXPos = Random.Range(-xLen / 2 + safetyOffset, xLen / 2 - safetyOffset);
        float newYPos = Random.Range(10, 100);
        float newZPos = Random.Range(-zLen / 2 + safetyOffset, zLen / 2 - safetyOffset);

        
        RaycastHit hit;
        raycastPos = new Vector3(newXPos, newYPos, newZPos) + transform.position;
        Physics.Raycast(origin: raycastPos, direction: Vector3.down, hitInfo: out hit, maxDistance: 250, spawnableLayers);
        //Debug.DrawRay(raycastPos, Vector3.down * Vector3.Distance(hit.point, raycastPos), new Color(0, .5f, 0, 0.5f), 100);
        return hit.point;
    }

    private int _freeBreaker = 0;
    private bool _testing = false;

    bool IsSpawnPointFree(Vector3 point)
    {
        if (point == Vector3.zero) return false;
        Vector3 sphereCheckPoint = new Vector3(point.x, point.y + 1, point.z);
        int colliderCount = Physics.OverlapSphereNonAlloc(sphereCheckPoint, 0, _dummyCollider);

        return colliderCount == 0;
    }

    bool IsPointWithinBounds(Vector3 point)
    {
        point -= transform.position;
        bool isXInBound = -xLen / 2 + safetyOffset <= point.x && point.x < xLen / 2 - safetyOffset;
        bool isYInBound = 0f <= point.y && point.y < 100f;
        bool isZInBound = -zLen / 2 + safetyOffset <= point.z && point.z < zLen / 2 - safetyOffset;
        return isXInBound && isYInBound && isZInBound;
    }
   
}
