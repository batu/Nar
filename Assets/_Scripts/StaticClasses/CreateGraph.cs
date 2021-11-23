using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

public static class CreateGraph
{
    private static Dictionary<string, int> _nameToType = new Dictionary<string, int>();
    public struct Graph
    {
        public string Name;
        public List<int> SourceNodes;
        public List<int> DestinationNodes;
        public List<Features> Features;

        public Graph(string name)
        {
            Name = name;
            SourceNodes = new List<int>();
            DestinationNodes = new List<int>();
            Features = new List<Features>();
        }

        struct JsonGraph
        {
            public List<int> SourceNodes;
            public List<int> DestinationNodes;
            public int NumNodes;
            public List<List<float>> Features;

            public JsonGraph(List<int> sourceNodes, List<int> destinationNodes, List<List<float>> features1D, int numNodes)
            {
                SourceNodes = sourceNodes;
                DestinationNodes = destinationNodes;
                Features = features1D;
                NumNodes = numNodes;
            }
        }

        public void WriteToJson()
        {
            
            JsonGraph jsonableGraph = new JsonGraph(SourceNodes, 
                DestinationNodes,
                CombineFeatures(Features),
                DestinationNodes.Max() + 1);
            
            string json = JsonConvert.SerializeObject(jsonableGraph);
            Debug.Log(json);
            File.WriteAllText($"{Name}_Graph.json", json);
            Debug.Log($"GraphNetwork created for {Name} at {Name}_Graph.json");
        }

        List<List<float>> CombineFeatures(List<Features> featuresList)
        {
            List<List<float>> combinedFeaturesList = new List<List<float>>();
            foreach (var feature in featuresList)
            {
                List<float> featureList = new List<float>();
                
                featureList.Add(feature.ID);
                featureList.Add(feature.typeID);

                featureList.Add(feature.absPos.x);
                featureList.Add(feature.absPos.y);
                featureList.Add(feature.absPos.z);
                
                featureList.Add(feature.absRot.x);
                featureList.Add(feature.absRot.y);
                featureList.Add(feature.absRot.z);
                featureList.Add(feature.absRot.w);
                
                featureList.Add(feature.absScale.x);
                featureList.Add(feature.absScale.y);
                featureList.Add(feature.absScale.z);

                featureList.Add(feature.locPos.x);
                featureList.Add(feature.locPos.y);
                featureList.Add(feature.locPos.z);
                
                featureList.Add(feature.locRot.x);
                featureList.Add(feature.locRot.y);
                featureList.Add(feature.locRot.z);
                featureList.Add(feature.locRot.w);
                
                featureList.Add(feature.locScale.x);
                featureList.Add(feature.locScale.y);
                featureList.Add(feature.locScale.z);

                combinedFeaturesList.Add(featureList);
            }
            return combinedFeaturesList;
        }
    }

    public struct Features
    {
        public Vector3 absPos;
        public Quaternion absRot;
        public Vector3 absScale;

        public Vector3 locPos;
        public Quaternion locRot;
        public Vector3 locScale;

        public int ID;
        public int typeID;

        public Features(Transform transform, int id)
        {
            this.absPos = transform.position;
            this.absRot = transform.rotation;
            this.absScale = transform.lossyScale;
            
            this.locPos = transform.localPosition;
            this.locRot = transform.localRotation;
            this.locScale = transform.localScale;
            
            this.ID = id;
            this.typeID = GetTypeID(transform);
        }

        private static int GetTypeID(Transform trans)
        {
            string strippedName = PrepareNamStrippedName(trans.name);
            return _nameToType[strippedName];
        }
    }

    static void UpdateNametoTypeDict(Transform root)
    {
        foreach (Transform child in root)
        {
            AddNodeNameToDict(child);
            UpdateNametoTypeDict(child);
        }
    }

    static void AddNodeNameToDict(Transform root)
    {
        string strippedName = root.name;
        strippedName = PrepareNamStrippedName(strippedName);
        if (_nameToType.ContainsKey(strippedName)) return;
        int typeID = _nameToType.Values.Max() + 1;
        _nameToType[strippedName] = typeID;
    }

    private static string PrepareNamStrippedName(string strippedName)
    {
        int index = strippedName.IndexOf("(", StringComparison.Ordinal);
        if (index >= 0)
            strippedName = strippedName.Substring(0, index);
        strippedName = strippedName.TrimEnd();
        return strippedName;
    }

    public static void CreateGraphStructure(Transform rootTransform)
    {
        _nameToType["Player"] = 0;
        UpdateNametoTypeDict(rootTransform);
        foreach (KeyValuePair<string, int> kvp in _nameToType)
        {
            Debug.Log($"{kvp.Key}, {kvp.Value}");
        }     
        Graph resultG = new Graph(rootTransform.name);
        AddChildrenToGraph(rootTransform, 0, resultG);
        resultG.WriteToJson();
    }

    static void AddChildrenToGraph(Transform root, int idx, Graph graph)
    {
        graph.Features.Add(new Features(root, idx));
        foreach (Transform child in root)
        {
            int childIdx = graph.DestinationNodes.Count == 0 ? 1 : graph.DestinationNodes.Max() + 1;
            graph.SourceNodes.Add(idx);
            graph.DestinationNodes.Add(childIdx);
            AddChildrenToGraph(child, childIdx, graph);
        }
    }
    
}
