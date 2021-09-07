using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class EpisodeDataset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private bool loadedCSV = false;
    List<DatasetCreator.AgentGoalPositions> EpisodePositions = new List<DatasetCreator.AgentGoalPositions>();
    private int currentIdx = 0;
    
    public DatasetCreator.AgentGoalPositions GetEpisodePair()
    {
        if (!loadedCSV)
        {
            loadCSV();
            EpisodePositions = EpisodePositions.OrderBy( x => Random.value ).ToList( );
            loadedCSV = true;
        }

        if (currentIdx == EpisodePositions.Count)
        {
            print("DONE!");
            Time.timeScale = 0;
            Debug.Break();
        }

        DatasetCreator.AgentGoalPositions thisEP = EpisodePositions[currentIdx];
        currentIdx++;
        return thisEP;
    }
    
    private void loadCSV()
    {
        using var reader = new StreamReader(@"export.csv");
        
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');

            Vector3 gPos = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            Vector3 aPos = new Vector3(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]));
            EpisodePositions.Add(new DatasetCreator.AgentGoalPositions(gPos, aPos));
        }
    }
}
