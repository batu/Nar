using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class EpisodeHandler : MonoBehaviour
{

    public Transform Agent;
    public Transform Goal;
    public Transform Ground;

    private float maxDistance { get; set; } = 1f;

    private float xLen;
    private float zLen;
    private float safetyOffset = 2.5f;

    Vector3 agentInitialPosition;
    Vector3 goalInitialPosition;

    private Collider[] dummyCollider = new Collider[0];

    private TrailRenderer tr;
    // Start is called before the first frame update
    private void Awake()
    {
        Time.timeScale = 1f;
        TestAgent agentScript = Agent.GetComponent<TestAgent>();
        agentScript.StartEpisode += RestartEpisode;
        
        xLen = Ground.GetComponent<BoxCollider>().bounds.size.x;
        zLen = Ground.GetComponent<BoxCollider>().bounds.size.z;
        maxDistance = Mathf.Sqrt(xLen * xLen + zLen * zLen);
        agentScript.maxDistance = maxDistance;

        agentInitialPosition = Agent.position;
        goalInitialPosition = Goal.position;
    }

    private void Start()
    {
        tr = Agent.GetComponentInChildren<TrailRenderer>();

    }

    public void RestartEpisode()
    {
        MoveGoalRandomly();
        MoveAgentRandomly();
        // MoveAgenttoInitialPlace();
        // MoveGoaltoInitialPlace();
    }

    void MoveGoalRandomly()
    {
        Vector3 raycastHitPos;
        do {
            raycastHitPos = SampleRandomSpawnPoint();
        } while (!isSpawnPointFree(raycastHitPos));
        Goal.position = raycastHitPos  + new Vector3(0, 2, 0);
    }
    
    void MoveAgentRandomly()
    {
        Vector3 raycastHitPos;
        do {
            raycastHitPos = SampleRandomSpawnPoint();
        } while (!isSpawnPointFree(raycastHitPos));
        Agent.position = raycastHitPos + new Vector3(0, 1, 0);
        tr.Clear();
    }

    void MoveAgenttoInitialPlace()
    {
        Agent.position = agentInitialPosition;
    }

    void MoveGoaltoInitialPlace()
    {
        Goal.position = goalInitialPosition;

    }

    private Vector3 SampleRandomSpawnPoint()
    {
        Vector3 raycastPos;
        float newXPos = Random.Range(-xLen / 2 + safetyOffset, xLen / 2 - safetyOffset);
        float newYPos = Random.Range(10, 50);
        float newZPos = Random.Range(-zLen / 2 + safetyOffset, zLen / 2 - safetyOffset);

        
        RaycastHit hit;
        raycastPos = new Vector3(newXPos, newYPos, newZPos) + transform.position;
        Physics.Raycast(origin: raycastPos, direction: Vector3.down, hitInfo: out hit, maxDistance: 250);

        return hit.point;
    }
    bool isSpawnPointFree(Vector3 point)
    {
        Vector3 sphereCheckPoint = new Vector3(point.x, point.y + 1, point.z);
        int colliderCount = Physics.OverlapSphereNonAlloc(sphereCheckPoint, 0, dummyCollider);
        return colliderCount == 0;
    }
}
