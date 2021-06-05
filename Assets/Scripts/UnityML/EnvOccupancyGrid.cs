using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "New Env Occupancy Grid")]
public class EnvOccupancyGrid : ScriptableObject
{

    [HideInInspector]
    public Transform env;
    public Dictionary<Vector3Int, bool> Occupancy;


    private EpisodeHandler _episodeHandler;
    public float visualizeDistance = 50f;
    public int boxSize = 2;
    public int offset = 3;

    private float edgeGuard =0.01f;
    private int _xHalfBoxCount;
    private int _yHalfBoxCount;
    private int _zHalfBoxCount;
    const float PlayerHeightAdjustment = 0.9f;

    public void CreateOccupancyDict()
    {

        Debug.Log("Created new occupancy dict."); 
        Occupancy = new Dictionary<Vector3Int, bool>();
        _episodeHandler = env.GetComponent<EpisodeHandler>();

        if (_episodeHandler == null)
        {
            Debug.LogError("The EnvOccupancyGrid couldn't find the _episodeHandler. This might be caused by the env not being set correctly." +
                           "The env is set in the OccupancyGridObservation.cs using transform.parent, meaning the parent of the PlayerAgent class needs " +
                           $"to be the Env (or the gameobject with _episodeHandler component) and currently it is {env.name}");
        }
        
        _episodeHandler.Agent.GetComponent<Collider>().enabled = false;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = false;

        _xHalfBoxCount = Mathf.CeilToInt((_episodeHandler.xLen + offset) / 2f) + 1;
        _yHalfBoxCount = Mathf.CeilToInt((100 + offset) / 2f) + 1;
        _zHalfBoxCount = Mathf.CeilToInt((_episodeHandler.zLen + offset) / 2f) + 1;
        for (int xIndex = -_xHalfBoxCount; xIndex <= _xHalfBoxCount; xIndex++)
        {
            for (int yIndex = -_yHalfBoxCount; yIndex <= _yHalfBoxCount; yIndex++)
            {
                for (int zIndex = -_zHalfBoxCount; zIndex <= _zHalfBoxCount; zIndex++)
                {
                    
                    Vector3 center = new Vector3(xIndex * boxSize, yIndex * boxSize, zIndex * boxSize);
                    Vector3 halfExtents = new Vector3(boxSize / 2f - edgeGuard, boxSize / 2f - edgeGuard, boxSize / 2f- edgeGuard);
                    bool overlap = Physics.OverlapBox(center, halfExtents, Quaternion.identity).Length > 0;
                    Vector3Int index = new Vector3Int(xIndex, yIndex, zIndex);
                    Occupancy[index] = overlap;
                }
            }
        }
        _episodeHandler.Agent.GetComponent<Collider>().enabled = true;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = true;

    }

    public void VisualizeOccupancy()
    { 
        if (Occupancy == null)
        {
            Debug.LogWarning("Can't visualize the occupancy because the occupancy dictionary is not created. " +
                             "Please play the scene and ensure CreateOccupancyDict is called to regenerate a " +
                             "occupancy grid scriptable object. ");
            return;
        }
        Vector3 extents = new Vector3(boxSize, boxSize, boxSize);
        foreach (var keyvalue in Occupancy)
        {        
            Vector3 center = keyvalue.Key * boxSize;
            bool overlap = keyvalue.Value;
            if (overlap && Vector3.Distance(Camera.current.transform.position, center) < visualizeDistance && InfiniteCameraCanSeePoint(Camera.current, center))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, extents);    
            }
        }
    }

    
    public void VisualizePlayerOccupancy(Vector3 playerPosition, int xCount, int yCount, int zCount)
    {
        if (Occupancy == null)
        {
            Debug.LogWarning("Can't visualize the occupancy because the occupancy dictionary is not created. " +
                             "Please play the scene and ensure CreateOccupancyDict is called to regenerate a " +
                             "occupancy grid scriptable object. ");
            return;
        }
        Vector3 extents = new Vector3(boxSize, boxSize, boxSize);
        for (int xIndex = -xCount; xIndex <= xCount; xIndex++)
        {
            for (int yIndex = -yCount; yIndex <= yCount; yIndex++)
            {
                for (int zIndex = -zCount; zIndex <= zCount; zIndex++)
                {
                    Vector3Int playerCurrentIndex = GetPlayerCurrentIndex(playerPosition);
                    Vector3Int key = playerCurrentIndex + new Vector3Int(xIndex, yIndex, zIndex);
                    bool overlap = Occupancy[key];
                    Gizmos.color = overlap ? Color.magenta : Color.green;
                    Gizmos.DrawWireCube(key * boxSize, extents);
                }
            }
        }
    }
    public float[] GetPlayerArea(Vector3 playerPosition, int xCount, int yCount, int zCount)
    {
        List<float> occupancyObservation = new List<float>();
        for (int xIndex = -xCount; xIndex <= xCount; xIndex++)
        {
            for (int yIndex = -yCount; yIndex <= yCount; yIndex++)
            {
                for (int zIndex = -zCount; zIndex <= zCount; zIndex++)
                {
                    Vector3Int playerCurrentIndex = GetPlayerCurrentIndex(playerPosition);
                    Vector3Int key = playerCurrentIndex + new Vector3Int(xIndex, yIndex, zIndex);

                    Occupancy.TryGetValue(key, out bool isOccupied);
                    float value = isOccupied ? 1 : 0;
                    occupancyObservation.Add(value);
                }
            }
        }
        return occupancyObservation.ToArray();
    }

    private Vector3Int GetPlayerCurrentIndex(Vector3 position)
    {
        Vector3 halfExtents = new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        int xIndex = (int) ((position.x + halfExtents.x) / boxSize); 
        int yIndex = (int) ((position.y + PlayerHeightAdjustment + halfExtents.y) / boxSize); 
        int zIndex = (int) ((position.z + halfExtents.z) / boxSize);

        xIndex = xIndex < 0 ? xIndex - 1 : xIndex;
        yIndex = yIndex < 0 ? yIndex - 1 : yIndex;
        zIndex = zIndex < 0 ? zIndex - 1 : zIndex;

        return new Vector3Int(xIndex, yIndex, zIndex);
    } 

    bool InfiniteCameraCanSeePoint (Camera camera, Vector3 point) {
        Vector3 viewportPoint = camera.WorldToViewportPoint(point);
        return (viewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint));
    }
}
