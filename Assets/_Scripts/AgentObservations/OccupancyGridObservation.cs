using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(NavigationAgent))]
public class OccupancyGridObservation : MonoBehaviour, IObservation
{
    public EnvOccupancyGrid _occupancyGrid;
    [Header("Observation Details")]
    public int OccupancyObsCountXZ = 4;
    public int OccupancyObsCountY = 2;

    private NavigationAgent _navigationAgent;
    private Transform _agentTransform;
    public bool visualizeOccupancyGrid = false;
    public bool visualizePlayerOccupancy = false;
    
    private void OnDrawGizmos()
    {
        if (_navigationAgent == null)
            _navigationAgent = GetComponent<NavigationAgent>();
    
        if (visualizeOccupancyGrid)
        {
            _occupancyGrid.VisualizeOccupancy(_navigationAgent.transform.parent.position);
        }
 
        if (visualizePlayerOccupancy)
        {
            _occupancyGrid.VisualizePlayerOccupancy(_navigationAgent.transform, OccupancyObsCountXZ, OccupancyObsCountY, OccupancyObsCountXZ );
        }
    }

    void Awake()
    {
        _navigationAgent = GetComponent<NavigationAgent>();
        _agentTransform = _navigationAgent.transform;
    }
    void Start()
    {  
        if (_occupancyGrid.Occupancy == null) 
        {
            if (transform.parent.GetComponent<EpisodeHandler>() == null)
            {
                Debug.LogWarning("The parent object should be the Environment. This might cause problems.");
            } 
            _occupancyGrid.env = transform.parent;
            _occupancyGrid.CreateOccupancyDict();
            print($"New Occupancy Grid has been created.");
        }
    }
  
    public float[] GetObservation() 
    {
        return _occupancyGrid.GetPlayerArea(_agentTransform.localPosition, OccupancyObsCountXZ, OccupancyObsCountY, OccupancyObsCountXZ);
    }
}
