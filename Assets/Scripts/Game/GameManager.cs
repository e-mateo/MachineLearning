using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    bool isPlaying;
    bool isRed = false;

    int nbDeadAI;
    int nbAI;

    float gameTime;
    float speed;
    float timerBeforeColorSwitch;

    Vector3 startingPosition;
    List<AI> AIs = new List<AI>();

    ObstacleManager obstacleManager;
    SpawnerObstacles spawnerObstacle;
    TMP_Text TimerText;
    TMP_Text TimerSwitch;

    public bool IsPlaying {  get { return isPlaying; } }
    public float Speed { get { return speed;} }
    public float GameTime { get { return gameTime; } }
    public float TimerBeforeColorSwitch { get { return timerBeforeColorSwitch; } }


    [SerializeField] private float baseSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedAccelerationBySeconds;
    [SerializeField] Transform RaycastBottom;
    [SerializeField] Transform RaycastTop;

    float[] distanceNearestObstacleBottom = new float[2];
    int[] layerObstacleBottom = new int[2];
    float[] distanceNearestObstacleTop = new float[2];
    int[] layerObstacleTop = new int[2];

    float maxDistanceObstacle;
    LayerMask obstacleRedMask;
    LayerMask obstacleBlueMask;

    public float[] DistanceNearestObstacleBottom { get { return distanceNearestObstacleBottom; } }
    public float[] DistanceNearestObstacleTop { get { return distanceNearestObstacleTop; } }
    public int[] LayerObstacleBottom { get { return layerObstacleBottom; } }
    public int[] LayerObstacleTop { get { return layerObstacleTop; } }
    public float MaxDistanceObstacle { get { return maxDistanceObstacle; } }


    // Start is called before the first frame update
    void Start()
    {
        isPlaying = true;
        obstacleManager = GetComponent<ObstacleManager>();
        spawnerObstacle = FindObjectOfType<SpawnerObstacles>();
        speed = baseSpeed;
        TimerText = GameObject.Find("Timer").GetComponent<TMP_Text>();
        TimerSwitch = GameObject.Find("ColorSwitchTimer").GetComponent<TMP_Text>();

        AIs = FindObjectsOfType<AI>().ToList();
        nbAI = AIs.Count;
        nbDeadAI = 0;

        if(AIs.Count > 0)
            startingPosition = AIs[0].transform.position;

        maxDistanceObstacle = 20f;
        obstacleRedMask = 11;
        obstacleBlueMask = 12;
        timerBeforeColorSwitch = Random.Range(5.0f, 10f);
        StartCoroutine(IncreaseSpeed());
        SwitchColors();
    }

    // Update is called once per frame
    void Update()
    {
        gameTime += Time.deltaTime;
        timerBeforeColorSwitch -= Time.deltaTime;

        SetTimerText();
        ComputeRaycastDistanceObstacle();

        if (timerBeforeColorSwitch < 0)
        {
            SwitchColors();
            timerBeforeColorSwitch = Random.Range(5.0f, 10f);
        }
    }
    IEnumerator IncreaseSpeed()
    {
        while(isPlaying && speed < maxSpeed)
        {
            speed += speedAccelerationBySeconds;
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void SwitchColors()
    {
        isRed = !isRed;

        foreach (AI ai in AIs)
        {
            ai.SwitchColor(isRed);
        }
    }


    private void SetTimerText()
    {
        int minutes = (int)(gameTime / 60);
        int seconds = (int)gameTime - (minutes * 60);
        int milli = (int)((gameTime - (int)gameTime) * 100);
        string minuteString = minutes.ToString();
        if (minutes < 10)
            minuteString = "0" + minuteString;

        string secondsString = seconds.ToString();
        if (seconds < 10)
            secondsString = "0" + secondsString;

        string milliString = milli.ToString();
        if (milli < 10)
            milliString = "0" + milliString;

        TimerText.SetText(minuteString + ":" + secondsString + ":" + milliString);

        TimerSwitch.SetText(timerBeforeColorSwitch.ToString().Substring(0,3) + "s");
    }

    private void ComputeRaycastDistanceObstacle()
    {
        RaycastHit2D[] HitBottom = Physics2D.RaycastAll(RaycastBottom.transform.position, RaycastBottom.transform.right, 20f, (1 << obstacleBlueMask | (1 << obstacleRedMask)));
        RaycastHit2D[] HitTop = Physics2D.RaycastAll(RaycastTop.transform.position, RaycastTop.transform.right, 20f, (1 << obstacleBlueMask | (1 << obstacleRedMask)));

        for (int i = 0; i < 2; i++)
        {
            if (HitBottom.Length > i)
            {
                distanceNearestObstacleBottom[i] = HitBottom[i].distance;
                layerObstacleBottom[i] = HitBottom[i].collider.gameObject.layer;
            }
            else
            {
                distanceNearestObstacleBottom[i] = maxDistanceObstacle;
                layerObstacleBottom[i] = -1;
            }

            if (HitTop.Length > i)
            {
                distanceNearestObstacleTop[i] = HitTop[i].distance;
                layerObstacleTop[i] = HitTop[i].collider.gameObject.layer;
            }
            else
            {
                distanceNearestObstacleTop[i] = maxDistanceObstacle;
                layerObstacleTop[i] = -1;
            }
        }
    }


    public void ResetGame()
    {
        obstacleManager.ResetObstacles();
        spawnerObstacle.StartSpawning();

        foreach(AI ai in AIs)
        {
            ai.ResetAI();
            ai.transform.position = startingPosition;
            ai.transform.rotation = new Quaternion();
        }

        speed = baseSpeed;
        gameTime = 0f;
        isPlaying = true;
        nbDeadAI = 0;
        StopAllCoroutines();
        StartCoroutine(IncreaseSpeed());
    }

    public void AddDeadAI()
    {
        nbDeadAI++;
        if(nbDeadAI == nbAI)
        {
            isPlaying = false;
        }
    }

    public float GetSpeedDifference() { return speed - baseSpeed; }
}
