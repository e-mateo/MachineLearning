using UnityEngine;

public class DestroyerObstacles : MonoBehaviour
{
    ObstacleManager obstacleManager;

    private void Start()
    {
        obstacleManager = FindObjectOfType<ObstacleManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8) // EndArea
        {
            obstacleManager.RemoveObstacle(collision.gameObject.transform.parent.gameObject);
        }
    }
}
