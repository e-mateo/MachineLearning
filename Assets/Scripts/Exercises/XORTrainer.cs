using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class XORTrainer : MonoBehaviour
{
    private float[,] trainingSet = new float[4,2];
    private float[] refOutputs = new float[4];

    private MLPNetwork MLPNet;


    private TMP_Text inputLabel_1;
    private TMP_Text inputLabel_2;
    private TMP_Text outputLabel;
    private TMP_InputField iterationsInput;
    private TMP_Text operatorText;

	void Start ()
    {
        inputLabel_1 = GameObject.Find("Input 1/Label").GetComponent<TMP_Text>();
        inputLabel_2 = GameObject.Find("Input 2/Label").GetComponent<TMP_Text>();
        outputLabel = GameObject.Find("Output/Text (TMP)").GetComponent<TMP_Text>();
        iterationsInput = GameObject.Find("Iterations").GetComponent<TMP_InputField>();
        operatorText = GameObject.Find("Operator Button/Text (TMP)").GetComponent<TMP_Text>();

        MLPNet = GetComponent<MLPNetwork>();

        // XOR operator training set
        // set input values
        trainingSet[0, 0] = 0f;
        trainingSet[0, 1] = 0f;
        trainingSet[1, 0] = 0f;
        trainingSet[1, 1] = 1f;
        trainingSet[2, 0] = 1f;
        trainingSet[2, 1] = 0f;
        trainingSet[3, 0] = 1f;
        trainingSet[3, 1] = 1f;
        // set expected output values
        refOutputs[0] = 0f;
        refOutputs[1] = 1f;
        refOutputs[2] = 1f;
        refOutputs[3] = 0f;

        operatorText.text = "XOR";
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

        MLPNet.GenerateOutput(inputList);
        outputLabel.text = MLPNet.GetOutputs()[0].State.ToString();

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

            for (int i = 0; i < 4; i++)
            {
                inputList.Clear();
                outputList.Clear();

                inputList.Add(trainingSet[i, 0]);
                inputList.Add(trainingSet[i, 1]);
                outputList.Add(refOutputs[i]);
                MLPNet.LearnPattern(inputList, outputList);
            }
        }
        Debug.Log("Network trained " + nbIterations  + " times!");
    }
}
