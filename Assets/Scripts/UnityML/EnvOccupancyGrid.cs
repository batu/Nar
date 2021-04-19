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
    [HideInInspector]
    public Dictionary<Vector3, bool> occupancy;

    private EpisodeHandler _episodeHandler;
    public int boxSize = 2;
    public int offset = 3;


    public void CreateOccupancyDict()
    {
        occupancy = new Dictionary<Vector3, bool>();
        _episodeHandler = env.GetComponent<EpisodeHandler>();

        _episodeHandler.Agent.GetComponent<Collider>().enabled = false;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = false;
        for (float x = -_episodeHandler.xLen/2f - offset; x <= _episodeHandler.xLen/2f + offset; x += boxSize)
        {
            for (float y = - offset + boxSize/2f; y <= 30f + offset; y += boxSize)
            {
                for (float z = -_episodeHandler.zLen / 2f - offset; z <= _episodeHandler.zLen / 2f + offset; z += boxSize)
                {
                    Vector3 center = new Vector3(x, y, z);
                    Vector3 halfExtents = new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
                    bool overlap = Physics.OverlapBox(center + halfExtents, halfExtents, Quaternion.identity).Length > 0;
                    occupancy[center + halfExtents] = overlap;
                }
            }
        }
        _episodeHandler.Agent.GetComponent<Collider>().enabled = true;
        _episodeHandler.Goal.GetComponent<Collider>().enabled = true;

    }

    public void VisualizeOccupancy()
    {
        for (float x = -_episodeHandler.xLen/2f - offset; x <= _episodeHandler.xLen/2f + offset; x += boxSize)
        {
            for (float y = - offset + boxSize/2f; y <= 30f + offset; y += boxSize)
            {
                for (float z = -_episodeHandler.zLen / 2f - offset; z <= _episodeHandler.zLen / 2f + offset; z += boxSize)
                {
                    Vector3 extents = new Vector3(boxSize, boxSize, boxSize);
                    Vector3 center = new Vector3(x, y, z) + extents/2f;
                    bool overlap = occupancy[center];

                    Gizmos.color = overlap ? Color.red : Color.white;
                    if (overlap) Gizmos.DrawWireCube(center, extents);
                }
            }
        }   
    }
    
    public void VisualizePlayerOccupancy(Vector3 playerPosition, int xCount, int yCount, int zCount)
    {
        Vector3 extents = new Vector3(boxSize, boxSize, boxSize);
        for (float x = -xCount * boxSize; x <= xCount * boxSize; x += boxSize)
        {
            for (float y = -yCount * boxSize; y <= yCount * boxSize; y += boxSize)
            {
                for (float z = -zCount * boxSize; z <= zCount * boxSize; z += boxSize)
                {
                    float xPosFloat = (x + playerPosition.x) / boxSize;
                    float yPosFloat = (y + playerPosition.y) / boxSize;
                    float zPosFloat = (z + playerPosition.z) / boxSize;

                    xPosFloat += xPosFloat <= 0 ? -1 : 0;
                    yPosFloat += yPosFloat <= 0 ? -1 : 0;
                    zPosFloat += zPosFloat <= 0 ? -1 : 0;

                    int xPos = (int) xPosFloat;
                    int yPos = (int) yPosFloat;
                    int zPos = (int) zPosFloat;

                    Vector3 key = new Vector3(xPos, yPos, zPos) * boxSize + extents/2f + new Vector3(0, boxSize/2f, 0);
                    
                    bool overlap = occupancy[key];
                    Gizmos.color = overlap ? Color.magenta : Color.green;
                    Gizmos.DrawWireCube(key, extents);
                }
            }
        }
    }

    public float[] GetPlayerArea(Vector3 playerPosition, int xCount, int yCount, int zCount)
    {
        Vector3 halfExtents = new Vector3(boxSize / 2f, boxSize / 2f, boxSize / 2f);
        List<float> occupancyObservation = new List<float>();
        for (float x = -xCount * boxSize; x <= xCount * boxSize; x += boxSize)
        {
            for (float y = -yCount * boxSize; y <= yCount * boxSize; y += boxSize)
            {
                for (float z = -zCount * boxSize; z <= zCount * boxSize; z += boxSize)
                {
                    float xPosFloat = (x + playerPosition.x) / boxSize;
                    float yPosFloat = (y + playerPosition.y) / boxSize;
                    float zPosFloat = (z + playerPosition.z) / boxSize;

                    xPosFloat += xPosFloat <= 0 ? -1 : 0;
                    yPosFloat += yPosFloat <= 0 ? -1 : 0;
                    zPosFloat += zPosFloat <= 0 ? -1 : 0;

                    int xPos = (int) xPosFloat;
                    int yPos = (int) yPosFloat;
                    int zPos = (int) zPosFloat;
                    Vector3 key = new Vector3(xPos, yPos, zPos) * boxSize + halfExtents + new Vector3(0, boxSize/2f, 0);
                    float value = occupancy[key] ? 1 : 0;
                    occupancyObservation.Add(value);
                }
            }
        }

        return occupancyObservation.ToArray();
    }
    
}
