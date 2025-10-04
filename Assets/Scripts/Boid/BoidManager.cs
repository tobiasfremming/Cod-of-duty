using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
   
    public static BoidManager Instance;

    public List<BoidAgent> AllAgents { get; private set; } = new List<BoidAgent>();
    
    [SerializeField] private float neighborRadius = 5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void RegisterAgent(BoidAgent agent)
    {
        AllAgents.Add(agent);
    }
    
    public void UnregisterAgent(BoidAgent agent)
    {
        AllAgents.Remove(agent);
    }
    
    public List<BoidAgent> GetNeighbors()
    {
        List<BoidAgent> neighbors = new List<BoidAgent>();
        foreach (var agent in Instance.AllAgents)
        {
            if (agent == this) continue;
            if (Vector3.Distance(transform.position, agent.transform.position) <= neighborRadius)
            {
                neighbors.Add(agent);
            }
        }
        return neighbors;
    }
}
