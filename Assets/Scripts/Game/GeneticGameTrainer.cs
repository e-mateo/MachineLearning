using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class GeneticGameTrainer : MonoBehaviour
{
    private List<AI> AIs = new List<AI>();
    private GameManager gameManager;

    [SerializeField] private bool ShouldTrain;
    [SerializeField] private int NumberOfWinners;
    [SerializeField] private int IndexCut;
    [SerializeField][Range(0, 1)] private float mutationRate;
    [SerializeField][Range(0, 1)] private float jumpActivation;

    LayerMask playerBlueMask = 9;
    LayerMask obstacleRedMask = 11;
    LayerMask obstacleBlueMask = 12;

    TMP_Text nbGenerationText;
    int nbGeneration;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        AIs = FindObjectsOfType<AI>().ToList<AI>();

        nbGeneration = 1;
        nbGenerationText = GameObject.Find("GenerationNb").GetComponent<TMP_Text>();
        nbGenerationText.text = nbGeneration.ToString();
    }

    private void Update()
    {
        TrainNetwork();
    }

    public void PlayGame()
    {
        foreach (AI ai in AIs)
        {
            if(ai.IsDead) continue;

            List<float> inputList = new List<float>();
            inputList.Clear();

            if(ai.IsOnBottomSide())
            {
                inputList.Add((float)gameManager.DistanceNearestObstacleBottom[0] / gameManager.MaxDistanceObstacle); //DistanceObstacleCurrentSide Nearest1
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleBottom[0]));

                inputList.Add((float)gameManager.DistanceNearestObstacleTop[0] / gameManager.MaxDistanceObstacle); //DistanceObstacleNextSide Nearest1
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleTop[0]));


                inputList.Add((float)gameManager.DistanceNearestObstacleBottom[1] / gameManager.MaxDistanceObstacle); //DistanceObstacleCurrentSide Nearest2
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleBottom[1]));

                inputList.Add((float)gameManager.DistanceNearestObstacleTop[1] / gameManager.MaxDistanceObstacle); //DistanceObstacleNextSide Nearest2
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleTop[1]));
            }
            else
            {
                inputList.Add((float)gameManager.DistanceNearestObstacleTop[0] / gameManager.MaxDistanceObstacle); //DistanceObstacleCurrentSide Nearest1
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleTop[0]));

                inputList.Add((float)gameManager.DistanceNearestObstacleBottom[0] / gameManager.MaxDistanceObstacle); //DistanceObstacleNextSide Nearest1
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleBottom[0]));

                inputList.Add((float)gameManager.DistanceNearestObstacleTop[1] / gameManager.MaxDistanceObstacle); //DistanceObstacleCurrentSide Nearest2
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleTop[1]));

                inputList.Add((float)gameManager.DistanceNearestObstacleBottom[1] / gameManager.MaxDistanceObstacle); //DistanceObstacleNextSide Nearest2
                inputList.Add(GetInputLayerColor(ai, gameManager.LayerObstacleBottom[1]));
            }

            if (ai.HasABlockAboveHim)
                inputList.Add(1.0f); 
            else
                inputList.Add(0.0f);

            ai.Network.GenerateOutput(inputList);
            if (ai.Network.GetOutputs()[0].State > jumpActivation)
            {
                ai.Jump();
            }
        }
    }

    private float GetInputLayerColor(AI ai, int layerObstacle)
    {
        int layerObstacleCollides = -1;
        if (ai.gameObject.layer == playerBlueMask)
            layerObstacleCollides = obstacleRedMask;
        else
            layerObstacleCollides = obstacleBlueMask;

        if (layerObstacleCollides == layerObstacle)
            return 1.0f; //Not the same color as the AI == collides
        else
            return 0.0f; //Same color as the AI == doesn't collide
    }

    public void TrainNetwork()
    {
        if (gameManager.IsPlaying)
        {
            PlayGame();
            return;
        }

        if(!ShouldTrain)
        {
            gameManager.ResetGame();
            return;
        }

        foreach (AI ai in AIs)
        {
            ai.Network.Score = FitnessFunction(ai);
        }

        List<AI> sortedAIsList = SortGeneration();

        if (nbGeneration < 3 && sortedAIsList[0].Network.Score - sortedAIsList[sortedAIsList.Count - 1].Network.Score < 0.2f && sortedAIsList[0].Network.Score < 5f)
        {
            //Bad generation
            foreach (AI ai in sortedAIsList)
                ai.Network.ResetNetwork();

            nbGeneration = 1;
            nbGenerationText.text = nbGeneration.ToString();
            gameManager.ResetGame();
            return;
        }

        List<AI> NewPopulation = CreateNewGeneration(sortedAIsList);

        //Step 5 : Set the new population and replay the game
        AIs.Clear();
        AIs = NewPopulation;

        gameManager.ResetGame();

        nbGeneration++;
        nbGenerationText.text = nbGeneration.ToString();
    }

    public List<AI> SortGeneration()
    {
        //Step 3 : Sort the population thanks to the FitnessFunction
        List<AI> sortedAIs = AIs.ToList().OrderBy(x => x.Network.Score).ToList();
        sortedAIs.Reverse();
        return sortedAIs;
    }

    public List<AI> CreateNewGeneration(List<AI> sortedGeneticMLPList)
    {
        //Step 4 : Create new population with Crossover and Mutation
        List<AI> NewPopulation = new List<AI>();
        //Conserve some winners
        for (int winnerIndex = 0; winnerIndex < NumberOfWinners; winnerIndex++)
            NewPopulation.Add(sortedGeneticMLPList[winnerIndex]);

        CopyBestTwoWithMutation(sortedGeneticMLPList, NewPopulation);
        CrossOverBestTwo(sortedGeneticMLPList, NewPopulation);

        int NewPopulationNumber = NewPopulation.Count;

        //Random Crossovers and mutations
        for (int indexSorted = NewPopulationNumber; indexSorted < sortedGeneticMLPList.Count; indexSorted++)
        {
            AI OtherAI = sortedGeneticMLPList[indexSorted];
            int RandomParentA = UnityEngine.Random.Range(0, NumberOfWinners);
            int RandomParentB = UnityEngine.Random.Range(0, NumberOfWinners);
            if (RandomParentA == RandomParentB)
            {
                RandomParentB++;
                if (RandomParentB >= NumberOfWinners)
                    RandomParentB = 0;
            }

            OtherAI.Network.CrossOver(NewPopulation[RandomParentA].Network, NewPopulation[RandomParentB].Network, IndexCut);
            OtherAI.Network.Mutation(mutationRate);
            NewPopulation.Add(OtherAI);
        }

        return NewPopulation;
    }

    private void CopyBestTwoWithMutation(List<AI> sortedGeneticMLPList, List<AI> NewPopulation)
    {
        AI BestA = sortedGeneticMLPList[NewPopulation.Count];
        AI BestB = sortedGeneticMLPList[NewPopulation.Count + 1];

        BestA.Network.CopyWeights(NewPopulation[0].Network);
        BestB.Network.CopyWeights(NewPopulation[1].Network);

        BestA.Network.Mutation(mutationRate);
        BestB.Network.Mutation(mutationRate);

        NewPopulation.Add(BestA);
        NewPopulation.Add(BestB);
    }


    private void CrossOverBestTwo(List<AI> sortedGeneticMLPList, List<AI> NewPopulation)
    {
        AI CrossoverBest1 = sortedGeneticMLPList[NewPopulation.Count];
        AI CrossoverBest2 = sortedGeneticMLPList[NewPopulation.Count + 1];

        CrossoverBest1.Network.CrossOver(NewPopulation[0].Network, NewPopulation[1].Network, IndexCut);
        CrossoverBest2.Network.CrossOver(NewPopulation[1].Network, NewPopulation[0].Network, IndexCut);

        CrossoverBest1.Network.Mutation(mutationRate);
        CrossoverBest2.Network.Mutation(mutationRate);

        NewPopulation.Add(CrossoverBest1);
        NewPopulation.Add(CrossoverBest2);
    }

    public float FitnessFunction(AI ai)
    {
        if (ai.IsDead)
            return ai.TimeAlive;
        else
            return gameManager.GameTime;
    }

    public void SaveBestAI()
    {
        foreach (AI ai in AIs)
        {
            ai.Network.Score = FitnessFunction(ai);
        }

        List<AI> sortedAIsList = SortGeneration();

        TMP_InputField pathField = FindObjectOfType<TMP_InputField>();
        sortedAIsList[0].Network.SaveNetwork(pathField.text);
    }
}
