using System.Collections.Generic;
using UnityEngine;

public class SpawnerObstacles : MonoBehaviour
{
    [SerializeField] List<GameObject> ObstaclePatterns = new List<GameObject>();

    ObstacleManager obstacleManager;

    float offsetSpawn;

    // Start is called before the first frame update
    void Start()
    {
        obstacleManager = FindObjectOfType<ObstacleManager>();
        offsetSpawn = 10f;
        StartSpawning();
    }

    public void StartSpawning()
    {
        if(obstacleManager?.GetNumberOfObstacles() == 0)
            SpawnPattern();
    }

    private void SpawnPattern()
    {
        int randomPattern = Random.Range(0, ObstaclePatterns.Count);
        GameObject pattern = ObstaclePatterns[randomPattern];
        BoxCollider2D colliderPattern = pattern.GetComponent<BoxCollider2D>();

        Vector3 spawnPoint = transform.position + (transform.right * (colliderPattern.size.x / 2f)) + (Vector3.right * offsetSpawn);
        GameObject CreatedObstacle = Instantiate(pattern, spawnPoint, new Quaternion());
        obstacleManager?.AddObstacle(CreatedObstacle);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8) // EndArea
        {
            SpawnPattern();
        }
    }
}
