using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeneticMLPNetwork : MonoBehaviour
{
    private List<Perceptron> inputNeutrons = new List<Perceptron>();
    private List<List<Perceptron>> hiddenNeutrons = new List<List<Perceptron>>();
    private List<Perceptron> outputNeutrons = new List<Perceptron>();

    [SerializeField] private int numberInputsNeutrons;
    [SerializeField] private int numberHiddenLayers;
    [SerializeField] private int numberHiddenNeutrons;
    [SerializeField] private int numberOutputsNeutrons;

    [SerializeField] private ActivationFunction hiddenActivationFunction;
    [SerializeField] private ActivationFunction outputActivationFunction;

    float score;
    public float Score { get { return score; } set { score = value; } }

    public string path;


    private void Start()
    {
        if(path.Length == 0)
            StartNetwork();
        else
            LoadNetwork(path);
    }

    public void StartNetwork()
    {
        for (int i = 0; i < numberInputsNeutrons; i++)
        {
            inputNeutrons.Add(new Perceptron());
        }

        for (int layerIndex = 0; layerIndex < numberHiddenLayers; layerIndex++)
        {
            hiddenNeutrons.Add(new List<Perceptron>()); // Add a layer

            for (int neutronIndex = 0; neutronIndex < numberHiddenNeutrons; neutronIndex++) // Add neutrons to the layer
            {
                if (layerIndex == 0)
                    hiddenNeutrons[layerIndex].Add(new Perceptron(inputNeutrons));
                else
                    hiddenNeutrons[layerIndex].Add(new Perceptron(hiddenNeutrons[layerIndex - 1]));
            }
        }

        for (int i = 0; i < numberOutputsNeutrons; i++)
        {
            outputNeutrons.Add(new Perceptron(hiddenNeutrons[numberHiddenLayers - 1]));
        }
    }

    public void ResetNetwork()
    {
        inputNeutrons.Clear();
        hiddenNeutrons.Clear();
        outputNeutrons.Clear();
        StartNetwork();
    }

    public void GenerateOutput(List<float> intputs)
    {
        for (int i = 0; i < inputNeutrons.Count; i++)
        {
            inputNeutrons[i].State = intputs[i];
        }

        for (int layerIndex = 0; layerIndex < hiddenNeutrons.Count; layerIndex++)
        {
            for (int neutronIndex = 0; neutronIndex < hiddenNeutrons[layerIndex].Count; neutronIndex++)
            {
                hiddenNeutrons[layerIndex][neutronIndex].FeedForward(hiddenActivationFunction);
            }
        }

        for (int i = 0; i < outputNeutrons.Count; i++)
        {
            outputNeutrons[i].FeedForward(outputActivationFunction);
        }
    }

    public void CrossOver(GeneticMLPNetwork ParentA, GeneticMLPNetwork ParentB, int indexCut)
    {
        List<Perceptron> AllNeutrons = GetListNeutronsWithWeights();
        List<Perceptron> AllNeutronsParentA = ParentA.GetListNeutronsWithWeights();
        List<Perceptron> AllNeutronsParentB = ParentB.GetListNeutronsWithWeights();

        if (indexCut >= AllNeutrons.Count)
            indexCut = AllNeutrons.Count / 2;

        /* Split the network in two part and for the first part copy weights and bias from parent A 
         * and for the second part copy weights and bias from parent B */

        for (int i = 0; i < AllNeutrons.Count; i++)
        {
            if (i < indexCut)
                AllNeutrons[i].CopyWeights(AllNeutronsParentA[i]);
            else
                AllNeutrons[i].CopyWeights(AllNeutronsParentB[i]);
        }
    }

    public void CopyWeights(GeneticMLPNetwork Parent)
    {
        List<Perceptron> AllNeutrons = GetListNeutronsWithWeights();
        List<Perceptron> AllNeutronsParent = Parent.GetListNeutronsWithWeights();

        for (int i = 0; i < AllNeutrons.Count; i++)
            AllNeutrons[i].CopyWeights(AllNeutronsParent[i]);
    }

    public void Mutation(float mutationRate)
    {
        List<Perceptron> perceptrons = GetListNeutrons();
        foreach (Perceptron perceptron in perceptrons)
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            if (randomValue < mutationRate)
                perceptron.Mutate();
        }
    }

    public List<Perceptron> GetListNeutrons()
    {
        List<Perceptron> perceptrons = new List<Perceptron>();
        for (int i = 0; i < numberInputsNeutrons; i++)
            perceptrons.Add(inputNeutrons[i]);

        for (int i = 0; i < numberHiddenLayers; i++)
            for (int j = 0; j < numberHiddenNeutrons; j++)
                perceptrons.Add(hiddenNeutrons[i][j]);

        for (int i = 0; i < numberOutputsNeutrons; i++)
            perceptrons.Add(outputNeutrons[i]);

        return perceptrons;
    }

    public List<Perceptron> GetListNeutronsWithWeights()
    {
        List<Perceptron> perceptrons = new List<Perceptron>();
        for (int i = 0; i < numberHiddenLayers; i++)
            for (int j = 0; j < numberHiddenNeutrons; j++)
                perceptrons.Add(hiddenNeutrons[i][j]);

        for (int i = 0; i < numberOutputsNeutrons; i++)
            perceptrons.Add(outputNeutrons[i]);

        return perceptrons;
    }

    public void SaveNetwork(string path)
    {
        int index = path.LastIndexOf("/");
        string folder = path.Substring(0, index);
        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        //Clear File
        System.IO.File.WriteAllText(path, "");

        //Write Data in text file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(numberInputsNeutrons);
        writer.WriteLine(numberHiddenLayers);
        writer.WriteLine(numberHiddenNeutrons);
        writer.WriteLine(numberOutputsNeutrons);
        writer.WriteLine((int)hiddenActivationFunction);
        writer.WriteLine((int)outputActivationFunction);

        List<Perceptron> allNeutrons = GetListNeutrons();
        foreach(Perceptron perceptron in allNeutrons)
        {
            writer.WriteLine(perceptron.Save());
        }

        writer.Close();
    }

    public void LoadNetwork(string path)
    {
        if (System.IO.File.Exists(path))
        {
            StreamReader reader = new StreamReader(path);
            int.TryParse(reader.ReadLine(), out numberInputsNeutrons);
            int.TryParse(reader.ReadLine(), out numberHiddenLayers);
            int.TryParse(reader.ReadLine(), out numberHiddenNeutrons);
            int.TryParse(reader.ReadLine(), out numberOutputsNeutrons);
            int activationHidden, activationOutput;
            int.TryParse(reader.ReadLine(), out activationHidden);
            int.TryParse(reader.ReadLine(), out activationOutput);
            hiddenActivationFunction = (ActivationFunction)activationHidden;
            outputActivationFunction = (ActivationFunction)activationOutput;

            StartNetwork();

            //Put right weight
            List<Perceptron> allNeutrons = GetListNeutrons();
            foreach (Perceptron perceptron in allNeutrons)
            {
                perceptron.Load(reader.ReadLine());
            }

            reader.Close();
        }
        else
        {
            Debug.LogWarning("Couldn't load the file " + path);
            StartNetwork();
        }

    }

    public List<Perceptron> GetOutputs() { return outputNeutrons; }
}
