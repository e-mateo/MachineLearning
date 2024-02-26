using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public class GeneticXORTrainer : MonoBehaviour
{
    private float[,] trainingSet = new float[4, 2];
    private float[] refOutputs = new float[4];

    private TMP_Text inputLabel_1;
    private TMP_Text inputLabel_2;
    private TMP_Text outputLabel;
    private TMP_InputField iterationsInput;
    private TMP_Text operatorText;

    private List<GeneticMLPNetwork> geneticMLPNetworks = new List<GeneticMLPNetwork>();

    [SerializeField] private int NumberNetworks;
    [SerializeField] private int NumberOfWinners;
    [SerializeField] private int IndexCut;
    [SerializeField][Range(0, 1)] private float mutationRate;



    void Start()
    {
        inputLabel_1 = GameObject.Find("Input 1/Label").GetComponent<TMP_Text>();
        inputLabel_2 = GameObject.Find("Input 2/Label").GetComponent<TMP_Text>();
        outputLabel = GameObject.Find("Output/Text (TMP)").GetComponent<TMP_Text>();
        iterationsInput = GameObject.Find("Iterations").GetComponent<TMP_InputField>();
        operatorText = GameObject.Find("Operator Button/Text (TMP)").GetComponent<TMP_Text>();

        // XOR operator training set
        // set input values
        trainingSet[0, 0] = 0f;
        trainingSet[0, 1] = 0f;
        trainingSet[1, 0] = 0f;
        trainingSet[1, 1] = 1f;
        trainingSet[2, 0] = 1f;
        trainingSet[2, 1] = 1f;
        trainingSet[3, 0] = 1f;
        trainingSet[3, 1] = 0f;
        // set expected output values
        refOutputs[0] = 0f;
        refOutputs[1] = 1f;
        refOutputs[2] = 0f;
        refOutputs[3] = 1f;

        operatorText.text = "XOR";

        geneticMLPNetworks = FindObjectsOfType<GeneticMLPNetwork>().ToList<GeneticMLPNetwork>();
    }

    public void ComputeOutput()
    {
        int input1, input2;
        if (int.TryParse(inputLabel_1.text, out input1) == false)
        {
            Debug.LogError("invalid input 1");
            return;
        }

        if (int.TryParse(inputLabel_2.text, out input2) == false)
        {
            Debug.LogError("invalid input 2");
            return;
        }

        List<float> inputList = new List<float>();
        inputList.Clear();
        inputList.Add((float)input1);
        inputList.Add((float)input2);

        geneticMLPNetworks[0].GenerateOutput(inputList);
        outputLabel.text = geneticMLPNetworks[0].GetOutputs()[0].State.ToString();

        Debug.Log("Network output generated!");
    }

    public void TrainNetwork()
    {
        int nbIterations, iterCounter = 0;

        if (int.TryParse(iterationsInput.text, out nbIterations) == false)
        {
            Debug.LogWarning("invalid NbIterations - setting 1000 iterations by default");
            nbIterations = 1000;
        }
        List<float> inputList = new List<float>();
        List<float> outputList = new List<float>();

        while (iterCounter++ != nbIterations)
        {
            Debug.Log("Train!");

            for (int i = 0; i < refOutputs.Length; i++)
            {
                inputList.Clear();
                outputList.Clear();
                inputList.Add(trainingSet[i, 0]);
                inputList.Add(trainingSet[i, 1]);
                outputList.Add(refOutputs[i]);

                PlayAndCalculateFitness(inputList, outputList);
                List<GeneticMLPNetwork> sortedGeneticMLPList = SortGeneration();
                List<GeneticMLPNetwork> NewPopulation = CreateNewGeneration(sortedGeneticMLPList);

                //Step 5 : Set the new population and replay the game
                geneticMLPNetworks.Clear();
                geneticMLPNetworks = NewPopulation;

                //Debug
                if (iterCounter == nbIterations)
                {
                    Console.Clear();
                    Debug.Log(i + ")   [" + inputList[0] + "," + inputList[1] + "] = "+ sortedGeneticMLPList[0].GetOutputs()[0].State + "   ---    Expected = " + refOutputs[i]);
                }
            }
        }

        Debug.Log("Network trained " + nbIterations + " times!");
    }

    private void PlayAndCalculateFitness(List<float> inputList, List<float> outputList)
    {
        foreach (GeneticMLPNetwork geneticNetwork in geneticMLPNetworks)
        {
            //Step 1 : Play the game
            geneticNetwork.GenerateOutput(inputList);

            //Step 2 : Calculate Fitness Function
            geneticNetwork.Score = FitnessFunction(geneticNetwork, outputList[0]);
        }
    }

    public List<GeneticMLPNetwork> SortGeneration()
    {
        //Step 3 : Sort the population thanks to the FitnessFunction
        List<GeneticMLPNetwork> sortedGeneticMLPList = geneticMLPNetworks.ToList().OrderBy(x => x.Score).ToList();
        sortedGeneticMLPList.Reverse();
        return sortedGeneticMLPList;
    }

    public List<GeneticMLPNetwork> CreateNewGeneration(List<GeneticMLPNetwork> sortedGeneticMLPList)
    {
        //Step 4 : Create new population with Crossover and Mutation
        List<GeneticMLPNetwork> NewPopulation = new List<GeneticMLPNetwork>();
        //Conserve some winners
        for (int winnerIndex = 0; winnerIndex < NumberOfWinners; winnerIndex++)
            NewPopulation.Add(sortedGeneticMLPList[winnerIndex]);

        //Crossover between two best 
        GeneticMLPNetwork GeneticMLPBestTwo1 = sortedGeneticMLPList[NumberOfWinners];
        GeneticMLPBestTwo1.CrossOver(NewPopulation[0], NewPopulation[1], IndexCut);
        GeneticMLPBestTwo1.Mutation(mutationRate);
        NewPopulation.Add(GeneticMLPBestTwo1);

        GeneticMLPNetwork GeneticMLPBestTwo2 = sortedGeneticMLPList[NumberOfWinners];
        GeneticMLPBestTwo2.CrossOver(NewPopulation[1], NewPopulation[0], IndexCut);
        GeneticMLPBestTwo2.Mutation(mutationRate);
        NewPopulation.Add(GeneticMLPBestTwo2);

        //Random Crossovers and mutations for the others
        for (int indexSorted = NumberOfWinners + 2; indexSorted < sortedGeneticMLPList.Count; indexSorted++)
        {
            GeneticMLPNetwork OtherGeneticMLP = sortedGeneticMLPList[indexSorted];
            int RandomParentA = UnityEngine.Random.Range(0, NumberOfWinners);
            int RandomParentB = UnityEngine.Random.Range(0, NumberOfWinners);
            if (RandomParentA == RandomParentB)
            {
                RandomParentB++;
                if (RandomParentB >= NumberOfWinners)
                    RandomParentB = 0;
            }

            OtherGeneticMLP.CrossOver(NewPopulation[RandomParentA], NewPopulation[RandomParentB], IndexCut);
            OtherGeneticMLP.Mutation(mutationRate);
            NewPopulation.Add(OtherGeneticMLP);
        }

        return NewPopulation;
    }

    public float FitnessFunction(GeneticMLPNetwork network, float ExpectedValue)
    {
        //Especially made for XOR, for other implementation we need to change it
        float error = ExpectedValue - network.GetOutputs()[0].State;
        return 1 - Mathf.Abs(error);
    }
}
