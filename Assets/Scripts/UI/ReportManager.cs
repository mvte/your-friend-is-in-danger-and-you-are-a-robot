using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReportManager : MonoBehaviour
{
    public TMP_Text shipDim;
    public TMP_Text numAliens;
    public TMP_Text numSims;
    public TMP_Text botLogic;
    public TMP_Text successes;
    public TMP_Text failures;
    public TMP_Text successRate;
    public TMP_Text avgStepsOnFailure;
    public TMP_Text timeElapsed;

    public void ShowReport(string shipDim, string numAliens, string numSims, string botLogic, string successes, string failures, string avgStepsOnFailure, string timeElapsed)
    {
        this.shipDim.text = shipDim;
        this.numAliens.text = numAliens;
        this.numSims.text = numSims;
        this.botLogic.text = botLogic;
        this.successes.text = successes;
        this.failures.text = failures;
        this.successRate.text = 100 * (float.Parse(successes) / float.Parse(numSims)) + "%";
        this.avgStepsOnFailure.text = avgStepsOnFailure;
        this.timeElapsed.text = timeElapsed;

        gameObject.SetActive(true);
    }

    public void ShowUntilFailureReport(string shipDim, string numAliens, string numSims, string botLogic)
    {
        this.shipDim.text = shipDim;
        this.numAliens.text = numAliens;
        this.numSims.text = numSims;
        this.botLogic.text = botLogic;
        this.successes.text = "N/A";
        this.failures.text = "N/A";
        this.successRate.text = "N/A";
        this.avgStepsOnFailure.text = "N/A";
        this.timeElapsed.text = "Report saved to simData.csv";

        gameObject.SetActive(true);
    }

    public void HideReport()
    {
        gameObject.SetActive(false);
    }
}
