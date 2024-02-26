using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
   GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += -transform.right * gameManager.Speed * Time.deltaTime;
    }

    void SetGameManager(GameManager manager) { gameManager = manager; }
}
