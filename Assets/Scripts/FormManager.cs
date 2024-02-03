using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormManager : MonoBehaviour
{   
    public Logic logicRef;
    public GameObject runningText;
    public GameObject onceButton;
    public GameObject manyButton;
    public int dimension = 32;
    public int botSelection = 0;
    public int alientCount = 32;
    public int simCount = 1;

    public void OnBotSelection(int index) {
        botSelection = index;
    }

    public void onDimensionChange(string value) {
        dimension = int.Parse(value);
    }

    public void onAlienCountChange(string value) {
        alientCount = int.Parse(value);
    }

    public void onSimCountChange(string value) {
        simCount = int.Parse(value);
    }

    public void onRunOnce() {
        logicRef.RunSimulation(dimension, botSelection, alientCount);
    }

    public void onRunSim() {
        logicRef.RunSimulation(dimension, botSelection, alientCount, simCount);
    }

    public void HideButtonsAndShowRunning() {
        onceButton.SetActive(false);
        manyButton.SetActive(false);
        runningText.SetActive(true);
    }

    public void ShowButtonsAndHideRunning() {
        onceButton.SetActive(true);
        manyButton.SetActive(true);
        runningText.SetActive(false);
    }


}
