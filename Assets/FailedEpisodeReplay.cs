using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailedEpisodeReplay : MonoBehaviour
{
    public struct EpisodeSpecification
    {
        public Vector3 AgentPos;
        public Vector3 GoalPos;

        public EpisodeSpecification(Vector3 agentPos, Vector3 goalPos)
        {
            AgentPos = agentPos;
            GoalPos = goalPos;
        }
    }
    
    private readonly List<EpisodeSpecification> _failedEpisodes = new List<EpisodeSpecification>();
    public float episodeThreshold = 0.55f;
    public float failedEpisodeStart = 0;
    public void AddFailedEpisode(EpisodeSpecification failedEpisode)
    {
        _failedEpisodes.Add(failedEpisode);
    }

    public bool CanGetFailedEpisode()
    {
        return _failedEpisodes.Count > 10;
    }

    public EpisodeSpecification GetFailedEpisode()
    {
        int randomIndex = Random.Range(0, _failedEpisodes.Count);
        EpisodeSpecification randomEpisode = _failedEpisodes[randomIndex];
        _failedEpisodes.RemoveAt(randomIndex);
        return randomEpisode;
    }
}
