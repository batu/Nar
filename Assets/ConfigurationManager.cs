using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour
{
    private EnvironmentConfiguration _environmentConfiguration;
    private AgentConfiguration _agentConfiguration;
    void Awake()
    {
        
        _agentConfiguration = GetComponent<AgentConfiguration>();
        _agentConfiguration.Configure();
        
        _environmentConfiguration = GetComponent<EnvironmentConfiguration>();
        _environmentConfiguration.Configure();

    }

}
