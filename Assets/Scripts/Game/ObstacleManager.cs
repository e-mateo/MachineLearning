using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    List<GameObject> currentObstacles = new List<GameObject>();

    public void AddObstacle(GameObject obstacle)
    {
        currentObstacles.Add(obstacle);
    }

    public void RemoveObstacle(GameObject obstacle)
    {
        currentObstacles.Remove(obstacle);
        Destroy(obstacle);
    }

    public void ResetObstacles()
    {
        for(int i = 0; i < currentObstacles.Count; i++)
        {
            Destroy(currentObstacles[i]);
        }
        currentObstacles.Clear();
    }

    public int GetNumberOfObstacles() { return currentObstacles.Count; }
}
