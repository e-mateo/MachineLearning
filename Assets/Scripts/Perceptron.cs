using System.Collections.Generic;
using UnityEngine;

public class Input
{
    public Input(Perceptron perceptron, float baseWeight)
    {
        inputPerceptron = perceptron;
        weight = baseWeight;
    }

    public Perceptron inputPerceptron;
    public float weight;
}

public class Perceptron
{
    private List<Input> inputs = new List<Input>();
    private float state = 0f;
    private float error = 0f;
    float weightBias = 1f;

    public float State { get { return state; } set { state = value; } }
    public float Error { get { return error; } }


    public Perceptron()
    {

    }

    public Perceptron(List<Perceptron> inputPerceptrons)
    {
        inputs.Clear();
        float RangeRandom = 0.3f;
        foreach (Perceptron inputPerceptron in inputPerceptrons)
        {
            Input input = new Input(inputPerceptron, UnityEngine.Random.Range(0f, RangeRandom) * 2f - RangeRandom);
            inputs.Add(input);
        }

        weightBias = UnityEngine.Random.Range(0f, RangeRandom) * 2f - RangeRandom;
    }


    public void FeedForward(ActivationFunction activationFunction)
    {
        float sum = 1f * weightBias;
        foreach (Input input in inputs)
            sum += input.inputPerceptron.State * input.weight;

        state = ComputeActivationFunction(activationFunction, sum);
    }

    public void AdjustWeight(float gain, float currentError)
    {
        for(int i = 0; i < inputs.Count; i++)
        {
            float deltaWeight = gain * currentError * inputs[i].inputPerceptron.State;
            inputs[i].weight += deltaWeight;
        }

        weightBias += gain * currentError;

        error = currentError;
    }

    public float GetIncomingWeight(Perceptron perceptron)
    {
        foreach (Input input in inputs)
        {
            if(input.inputPerceptron == perceptron)
                return input.weight;
        }

        return 0;
    }

    public float ComputeActivationFunction(ActivationFunction activationFunction, float x)
    {
        if (activationFunction == ActivationFunction.SIGMOIDE)
            return Sigmoid(x);
        else if (activationFunction == ActivationFunction.TANH)
            return Tanh(x);
        else if (activationFunction == ActivationFunction.RELU)
            return ReLU(x);

        return 0.0f;
    }

    public void Mutate()
    {
        //Tweak randomly weights
        foreach (Input input in inputs)
        {
            float deltaWeight = UnityEngine.Random.Range(-0.1f, 0.1f);
            input.weight += deltaWeight;
        }

        float deltaWeightBias = UnityEngine.Random.Range(-0.1f, 0.1f);
        weightBias += deltaWeightBias;
    }

    public void CopyWeights(Perceptron other)
    {
        weightBias = other.weightBias;
        for (int i = 0; i < inputs.Count; i++)
            inputs[i].weight = other.inputs[i].weight;
    }

    public void SetInputs(List<Perceptron> inputPerceptrons)
    {
        inputs = new List<Input>();
        foreach (Perceptron inputPerceptron in inputPerceptrons)
        {
            Input input = new Input(inputPerceptron, inputPerceptron.weightBias);
            inputs.Add(input);
        }
    }

    public string Save()
    {
        string Data = weightBias.ToString();
        foreach(Input input in inputs)
        {
            Data += ";" + input.weight;
        }

        return Data;
    }

    public void Load(string Data)
    {
        string[] dataSplit = Data.Split(';');
        for(int i = 0; i < dataSplit.Length; i++)
        {
            if (i == 0)
                float.TryParse(dataSplit[i], out weightBias);
            else
                float.TryParse(dataSplit[i], out inputs[i - 1].weight);
        }
    }

    public static float Sigmoid(float input)
    {
        return (1.0f / (1.0f + Mathf.Exp(-input)));
    }
    public static float SigmoidDerivative(float input)
    {
        return Sigmoid(input) * (1 - Sigmoid(input));
    }
    public static float SimplifiedSigmoidDerivative(float sigmoidValue)
    {
        return sigmoidValue * (1 - sigmoidValue);
    }
    public static float Tanh(float input)
    {
        return (Mathf.Exp(input) - Mathf.Exp(-input)) / (Mathf.Exp(input) + Mathf.Exp(-input));
    }
    public static float NormalizedTanh(float input)
    {
        return (Tanh(input) + 1f) / 2f;
    }
    public static float SimplifiedTanhDerivative(float input)
    {
        return (1 - input * input) / 2.0f;
    }
    public static float ReLU(float input)
    {
        return Mathf.Max(0f, input);
    }
    public static float ReLUDerivative(float input)
    {
        return input > 0f ? 1f : 0f;
    }
}
