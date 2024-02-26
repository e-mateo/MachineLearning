using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AttackEnemyTrainer : MonoBehaviour
{
    private float[,] trainingSet = new float[7,3];
    private float[] refOutputs = new float[7];

    private MLPNetwork MLPNet;

    private Scrollbar inputScrollBar_1;
    private Scrollbar inputScrollBar_2;
    private Scrollbar inputScrollBar_3;
    private Scrollbar outputScrollBar;

    private TMP_InputField iterationsInput;
    private TMP_Text operatorText;

	void Start ()
    {
        inputScrollBar_1 = GameObject.Find("Input1").GetComponent<Scrollbar>();
        inputScrollBar_2 = GameObject.Find("Input2").GetComponent<Scrollbar>();
        inputScrollBar_3 = GameObject.Find("Input3").GetComponent<Scrollbar>();
        outputScrollBar = GameObject.Find("Output").GetComponent<Scrollbar>();
        iterationsInput = GameObject.Find("Iterations").GetComponent<TMP_InputField>();
        operatorText = GameObject.Find("Operator Button/Text (TMP)").GetComponent<TMP_Text>();

        MLPNet = GetComponent<MLPNetwork>();

        // set input values
        trainingSet[0, 0] = 0f;
        trainingSet[0, 1] = 0f;
        trainingSet[0, 2] = 0f;

        trainingSet[1, 0] = 0f;
        trainingSet[1, 1] = 0f;
        trainingSet[1, 2] = 1f;

        trainingSet[2, 0] = 0f;
        trainingSet[2, 1] = 1f;
        trainingSet[2, 2] = 0f;

        trainingSet[3, 0] = 0f;
        trainingSet[3, 1] = 1f;
        trainingSet[3, 2] = 1f;

        trainingSet[4, 0] = 1f;
        trainingSet[4, 1] = 0f;
        trainingSet[4, 2] = 0f;

        trainingSet[5, 0] = 1f;
        trainingSet[5, 1] = 1f;
        trainingSet[5, 2] = 0f;

        trainingSet[6, 0] = 1f;
        trainingSet[6, 1] = 1f;
        trainingSet[6, 2] = 1f;

        // set expected output values
        refOutputs[0] = 0f;
        refOutputs[1] = 0f;
        refOutputs[2] = 0.5f;
        refOutputs[3] = 0f;
        refOutputs[4] = 0.5f;
        refOutputs[5] = 1f;
        refOutputs[6] = 1f;

        operatorText.text = "Result";
    }

    public void ComputeOutput()
    {
        float input1, input2, input3;
        input1 = inputScrollBar_1.value;
        input2 = inputScrollBar_2.value;
        input3 = inputScrollBar_3.value;

        List<float> inputList = new List<float>();
        inputList.Clear();
        inputList.Add((float)input1);
        inputList.Add((float)input2);
        inputList.Add((float)input3);

        MLPNet.GenerateOutput(inputList);
        outputScrollBar.value = (float)MLPNet.GetOutputs()[0].State;

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
                inputList.Add(trainingSet[i, 0]);
                inputList.Add(trainingSet[i, 1]);
                inputList.Add(trainingSet[i, 2]);

                outputList.Clear();
                outputList.Add(refOutputs[i]);
                MLPNet.LearnPattern(inputList, outputList);
            }
        }
        Debug.Log("Network trained " + nbIterations  + " times!");
    }
}
