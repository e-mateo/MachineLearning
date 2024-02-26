using System.Collections.Generic;
using UnityEngine;

public enum ActivationFunction
{ 
    SIGMOIDE,
    TANH,
    RELU,
}
public class MLPNetwork : MonoBehaviour
{
    private List<Perceptron> inputNeutrons = new List<Perceptron>();
    private List<List<Perceptron>> hiddenNeutrons = new List<List<Perceptron>>();
    private List<Perceptron> outputNeutrons = new List<Perceptron>();

    [SerializeField] private int numberInputsNeutrons;
    [SerializeField] private int numberHiddenLayers;
    [SerializeField] private int numberHiddenNeutrons;
    [SerializeField] private int numberOutputsNeutrons;
    [SerializeField] private float gain;

    [SerializeField] private ActivationFunction hiddenActivationFunction;
    [SerializeField] private ActivationFunction outputActivationFunction;


    private void Start()
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
                if(layerIndex == 0)
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

    public void LearnPattern(List<float> inputList, List<float> outputList)
    {
        GenerateOutput(inputList);
        BackPropagation(outputList);
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

    
    public float ComputeDerivative(bool isHiddenLayer, float input)
    {
        ActivationFunction type = isHiddenLayer ? hiddenActivationFunction : outputActivationFunction;
        switch (type)
        {
            case ActivationFunction.RELU:
                return Perceptron.ReLUDerivative(input);
            case ActivationFunction.SIGMOIDE:
                return Perceptron.SimplifiedSigmoidDerivative(input);
            case ActivationFunction.TANH:
                return Perceptron.SimplifiedTanhDerivative(input);
            default:
                return 0f;
        }
    }

    public void BackPropagation(List<float> outputs)
    {
        for (int i = 0; i < outputNeutrons.Count; i++)
        {
            Perceptron output = outputNeutrons[i];
            float state = output.State;
            float error = ComputeDerivative(false, state) * (outputs[i] - state);
            output.AdjustWeight(gain, error);
        }

        for (int layerIndex = hiddenNeutrons.Count - 1; layerIndex >= 0; layerIndex--)
        {
            for (int neutronIndex = 0; neutronIndex < hiddenNeutrons[layerIndex].Count; neutronIndex++)
            {
                Perceptron hidden = hiddenNeutrons[layerIndex][neutronIndex];
                float state = hidden.State;

                float sum = 0;

                if(layerIndex == hiddenNeutrons.Count - 1)
                {
                    for (int outputIndex = 0; outputIndex < outputNeutrons.Count; outputIndex++)
                        sum += outputNeutrons[outputIndex].GetIncomingWeight(hidden) * outputNeutrons[outputIndex].Error;
                }
                else
                {
                    for (int outputIndex = 0; outputIndex < hiddenNeutrons[layerIndex + 1].Count; outputIndex++)
                        sum += hiddenNeutrons[layerIndex + 1][outputIndex].GetIncomingWeight(hidden) * hiddenNeutrons[layerIndex + 1][outputIndex].Error;
                }


                float error = ComputeDerivative(true, state) * sum;

                hidden.AdjustWeight(gain, error);
            }
        }
    }


    public List<Perceptron> GetOutputs() { return outputNeutrons; } 
}
