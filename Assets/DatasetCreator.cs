using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DatasetCreator : MonoBehaviour
{
    private NavigationAgent _navigationAgent;
    private PlayerCharacterController _characterController;

    private Transform _goal;
    private EpisodeHandler _episodeHandler;

    public List<AgentGoalPositions> _AgentGoalPositionsList = new List<AgentGoalPositions>();

    public struct AgentGoalPositions
    {
        public Vector3 goalPosition;
        public  Vector3 agentPosition;

        public AgentGoalPositions(Vector3 goalPosition, Vector3 agentPosition)
        {
            this.goalPosition = goalPosition;
            this.agentPosition = agentPosition;
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        _episodeHandler = transform.parent.GetComponent<EpisodeHandler>();
        _navigationAgent = GetComponent<NavigationAgent>();
        _characterController = GetComponent<PlayerCharacterController>();

        _goal = _navigationAgent.goal.transform;
    }

    // Update is called once per frame
    private bool CSVWritten = false;
    void FixedUpdate()
    {
        if (_characterController.isGrounded && _navigationAgent.StepCount % 2 == 0)
        {
            _AgentGoalPositionsList.Add(new AgentGoalPositions(_goal.position, transform.position));
            print(_AgentGoalPositionsList.Count);
        }

        if (!CSVWritten && _episodeHandler.goalPositionIdx == _episodeHandler.goalPositions.Length + 1)
        {
            SaveCSV();
        }
    }

    private void SaveCSV()
    {
        var sb = new StringBuilder("Time,Value");
        foreach(AgentGoalPositions p in _AgentGoalPositionsList)
        {
            sb.Append('\n').Append($"{p.goalPosition.x},{p.goalPosition.y},{p.goalPosition.z},{p.agentPosition.x},{p.agentPosition.y},{p.agentPosition.z}");
        }

        string content = sb.ToString();
        var filePath = "export.csv";

        using(var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }
        print("CSV is written down!");
        CSVWritten = true;
    }
}