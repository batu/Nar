using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "New Env Occupancy Grid")]
public class EnvOccupancyGrid : ScriptableObject
{

    [HideInInspector]
    public Transform env;
    public Dictionary<Vector3Int, bool> Occupancy;
    public float[] occupancyArray;


    private EpisodeHandler _episodeHandler;
    public float visualizeDistance = 50f;
    public int boxSize = 2;
    public int offset = 5;

    private float edgeGuard =0.01f;
    private int _xHalfBoxCount;
    private int _yHalfBoxCount;
    private int _zHalfBoxCount;
    const float PlayerHeightAdjustment = 0.9f;

    private int _width;
    private int _height;
    private int _depth;
    private Vector3 _envPosition;
    public void CreateOccupancyDict()
    {

        Debug.Log("Created new occupancy dict."); 
        Occupancy = new Dictionary<Vector3Int, bool>();
        _episodeHandler = env.GetComponent<EpisodeHandler>();
        _envPosition = env.position;

        if (env.rotation.y != 0)
        {
            Debug.LogError("The rotation of the environment needs to be set to 0!");
        }

        if (_episodeHandler == null)
        {
            Debug.LogError("The EnvOccupancyGrid couldn't find the _episodeHandler. This might be caused by the env not being set correctly." +
                           "The env is set in the OccupancyGridObservation.cs using transform.parent, meaning the parent of the PlayerAgent class needs " +
                           $"to be the Env (or the gameobject with _episodeHandler component) and currently it is {env.name}");
        }
        
        _episodeHandler.Agent.GetComponent<Collider>().enabled = false;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = false;

        _xHalfBoxCount = Mathf.CeilToInt((_episodeHandler.xLen) / (boxSize * 2f) + offset);
        _yHalfBoxCount = Mathf.CeilToInt((100) / (boxSize * 2f) + offset);
        _zHalfBoxCount = Mathf.CeilToInt((_episodeHandler.zLen) / (boxSize * 2f) + offset);

        _width  = (_xHalfBoxCount) * 2 + 1;
        _height = (_yHalfBoxCount) * 2 + 1;
        _depth  = (_zHalfBoxCount) * 2 + 1;
        Debug.Log($"Width, height, depth: {_width}, {_height}, {_depth}"); 

        int lengthOfArray = _width * _height * _depth;
        occupancyArray = new float[lengthOfArray]; 
        Debug.Log($"This is the calculated count: {lengthOfArray}"); 
        int c = 0;
        for (int xIndex = -_xHalfBoxCount; xIndex <= _xHalfBoxCount; xIndex++)
        {
            for (int yIndex = -_yHalfBoxCount; yIndex <= _yHalfBoxCount; yIndex++)
            {
                for (int zIndex = -_zHalfBoxCount; zIndex <= _zHalfBoxCount; zIndex++)
                {
                    Vector3 center = new Vector3(xIndex * boxSize, yIndex * boxSize, zIndex * boxSize) + _envPosition;
                    Vector3 halfExtents = new Vector3(boxSize / 2f - edgeGuard, boxSize / 2f - edgeGuard, boxSize / 2f - edgeGuard);
                    bool overlap = Physics.OverlapBox(center, halfExtents, Quaternion.identity).Length > 0;
                    
                    int index = GetFlatArrayIndex(xIndex, yIndex, zIndex);
                    if (index >= occupancyArray.Length)
                    {
                        Debug.Log(new Vector3Int(xIndex, yIndex, zIndex));
                        Debug.Log(new Vector3Int(_xHalfBoxCount, _yHalfBoxCount, _zHalfBoxCount));
                        Debug.Log(index); 
                    }
                    occupancyArray[index] = overlap ? 1f : 0f;
                    c++;
                }
            }
        }
        Debug.Log($"This is the actual count: {c}");
        _episodeHandler.Agent.GetComponent<Collider>().enabled = true;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = true;

    }

    int GetFlatArrayIndex(int xIndex, int yIndex, int zIndex)
    {
        xIndex += _xHalfBoxCount;
        yIndex += _yHalfBoxCount;
        zIndex += _zHalfBoxCount;
        return xIndex + _width * (yIndex + _height * zIndex); 
    } 

    public void VisualizeOccupancy(Vector3 envPosition)
    { 
        if (occupancyArray == null)  
        {
            Debug.LogWarning("Can't visualize the occupancy because the occupancy dictionary is not created. " +
                             "Please play the scene and ensure CreateOccupancyDict is called to regenerate a " +
                             "occupancy grid scriptable object. ");
            return;
        }
        
        Vector3 extents = new Vector3(boxSize, boxSize, boxSize);
        for (int xIndex = -_xHalfBoxCount; xIndex <= _xHalfBoxCount; xIndex++)
        {
            for (int yIndex = -_yHalfBoxCount; yIndex <= _yHalfBoxCount; yIndex++)
            {
                for (int zIndex = -_zHalfBoxCount; zIndex <= _zHalfBoxCount; zIndex++)
                {
                    int index = GetFlatArrayIndex(xIndex, yIndex, zIndex);
                    Vector3 center = new Vector3(xIndex, yIndex, zIndex) * boxSize + envPosition;
                    bool overlap = occupancyArray[index] == 1f;
                    if (overlap && Vector3.Distance(Camera.current.transform.position, center) < visualizeDistance && InfiniteCameraCanSeePoint(Camera.current, center))
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(center, extents);    
                    }
                }
            }
        }
    }
    public void VisualizeOccupancy2()
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

    
    public void VisualizePlayerOccupancy(Transform playerTransform, int xCount, int yCount, int zCount)
    {
        Vector3 playerPosition = playerTransform.localPosition;
        Vector3 envPosition = playerTransform.parent.position;
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
                    int index = GetFlatArrayIndex(key.x, key.y, key.z);
                    bool overlap = occupancyArray[index] == 1f;
                    
                    Gizmos.color = overlap ? Color.magenta : Color.green; 
                    Gizmos.DrawWireCube(key * boxSize + envPosition, extents);
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
                float value = 0;
                for (int zIndex = -zCount; zIndex <= zCount; zIndex++) 
                {
                    Vector3Int playerCurrentIndex = GetPlayerCurrentIndex(playerPosition);
                    int index = GetFlatArrayIndex(playerCurrentIndex.x + xIndex, playerCurrentIndex.y + yIndex, playerCurrentIndex.z + zIndex);
                    try
                    {
                        value = occupancyArray[index]; 
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Debug.LogError($"Error when looking up the occupancy box. The player pos:{playerPosition} and index: {index}. It is possible the agent fell went out of bounds. Handled as empty occupancy.");
                    }  
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
