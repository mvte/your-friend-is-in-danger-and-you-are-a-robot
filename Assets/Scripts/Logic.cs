using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class Logic : MonoBehaviour
{
    public ShipManager ship;
    public Bot1 bot1Ref;
    public Bot2 bot2Ref;
    public Bot3 bot3Ref;
    public Bot4 bot4Ref;
    public GameObject cam;
    public FormManager formManager;
    public ReportManager reportManager;
    public bool running;
    public int runs = 1;
    public int MAX_STEPS = 1000;

    private List<Bot> bots;
    // per run
    private int steps;
    // per simulation
    private int successes;
    private int failures;
    private List<int> stepsOnFailure = new List<int>();

    private float accumulatedTime = 0;
    private float timeStart;
    private bool animate;

    // per set of simulations
    private SimDataWriter simDataWriter;

    //config info
    private int configDim = 32;
    private bool configRunUntilFailure = false;
    private int configAlienCount = 32;
    private int configSimCount = 1;
    private int configBotSelection = 0;

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;

        ship.Init(bot1Ref);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;

        bots = new List<Bot>{
            bot1Ref,
            bot2Ref,
            bot3Ref,
            bot4Ref,
        };
    } 

    // run a simulation with the given parameters
    public void RunSimulation(int dim = 32, int botSelection = 0, int alienCount = 32, int simCount = 1, bool runUntilFailure = false) {    
        // clean up (idk why reset isn't working)
        GameObject.Destroy(GameObject.Find("Fleet"));
        GameObject.Destroy(GameObject.Find("Bot"));
        GameObject.Destroy(GameObject.Find("Captain(Clone)"));
        
        // set start time and update status
        Debug.Log("Simulation started");
        timeStart = Time.time;
        Debug.Log("Running until failure: " + runUntilFailure);
        formManager.ShowStatus("Running...");

        // set up the ship
        ship.Reset(); // it's not
        ship.Reset(); // working!!!
        ship.PregenerateShips(dim, simCount);
        ship.Init(bots[botSelection], dim, alienCount);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;
        ship.Ready();

        // set parameters for the run
        animate = simCount == 1;
        stepsOnFailure = new List<int>();
        steps = 0;
        successes = 0;
        failures = 0;
        runs = simCount;
        running = true;

        // set parameters for the simulation
        configDim = dim;
        configRunUntilFailure = runUntilFailure;
        configSimCount = simCount;
        configAlienCount = alienCount;
        configBotSelection = botSelection;
    }

    // end the current run 
    public void EndRun(bool success) {
        running = false;

        // update the success and failure counts
        if(success) {
            successes++;
        } else {
            failures++;
            stepsOnFailure.Add(steps);
        }

        // update counters
        steps = 0;
        runs--;
        if(runs == 0) {
            EndSimulation();
            return;
        }
        
        // attempt to destroy the previous ship
        GameObject.Destroy(GameObject.Find("Fleet"));
        GameObject.Destroy(GameObject.Find("Bot"));
        GameObject.Destroy(GameObject.Find("Captain(Clone)"));

        // set up the next run
        ship.Reset(); // :(
        ship.Reset();
        ship.Init(bots[configBotSelection]);
        ship.Ready();

        // update the status
        running = true;
    }

    // end the current simulation
    public void EndSimulation() {
        // calculate the average steps on failure
        float avgStepsOnFailure = 0;
        foreach(int steps in stepsOnFailure) {
            avgStepsOnFailure += steps;
        }
        avgStepsOnFailure /= stepsOnFailure.Count;
        string botName = bots[configBotSelection].botName;

        // if we are running until failure, we need to write the data and run the next simulation
        if(configRunUntilFailure) {
            // write the data from the current simulation
            float successRate = (float)successes / configSimCount * 100;
            simDataWriter ??= new SimDataWriter(bots[configBotSelection].botName);
            simDataWriter.Write(new SimData(configAlienCount, successRate, avgStepsOnFailure));

            // if the success rate is 0, we are done
            if(successRate == 0) {
                Debug.Log("Simulation Ended (Until Failure)");
                formManager.ShowButtonsAndHideRunning();
                reportManager.ShowUntilFailureReport(
                    // config
                    ship.dim.ToString(),
                    configAlienCount.ToString(),
                    configSimCount.ToString(), 
                    botName
                );
                
                simDataWriter = null;
                return;
            }

            // go next
            RunSimulation(configDim, configBotSelection, configAlienCount + 1, configSimCount, configRunUntilFailure);
            return;
        }

        // if we are not running until failure, we are done
        Debug.Log("Simulation Ended");
        formManager.ShowButtonsAndHideRunning();
        reportManager.ShowReport(
            // config
            ship.dim.ToString(),
            configAlienCount.ToString(),
            configSimCount.ToString(), 
            botName, 
            // results
            successes.ToString(), 
            failures.ToString(), 
            avgStepsOnFailure.ToString(), 
            (Time.time - timeStart).ToString()
        );
    }


    // Update is called once per frame
    void Update()
    {   
        // if the simulation is not running, we don't need to do anything
        if(!running) {
            return;
        }
        // we end the run if max steps have been reached
        if(steps >= MAX_STEPS) {
            Debug.Log("Max steps reached");
            EndRun(false);
            return;
        }
        // slows down the simulation for visual purposes
        if(animate) {
            accumulatedTime += Time.deltaTime;
            if(accumulatedTime < 0.2f) {
                return;
            }
        }
        accumulatedTime = 0;

        // advance the simulation by one time step
        Step();
    }
    
    /**
    * Advances the simulation by one step
    */
    private void Step() {
        // advance the bot
        ship.bot.computeNextStep(ship);

        // determine if the bot is on an alien
        Node botNode = ship.GetNode(ship.bot.pos);
        if(botNode.occupied) {
            EndRun(false);
            return;
        }
        
        // determine if the bot is on the captain
        if(ship.bot.pos == ship.captain.pos) {
            EndRun(true);
            return;
        }

        // advance aliens
        List<Alien> randomizedAliens = new List<Alien>(ship.aliens);
        randomizedAliens.Shuffle();
        foreach(Alien alien in randomizedAliens) {
            alien.computeNextStep(ship);
        }

        // check if the bot is on an alien
        if(botNode.occupied) {
            EndRun(false);
            return;
        }

        steps++;
    }
}

// defines a simulation data point
class SimData {
    public int numAliens;
    public float successRate;
    public float avgStepsOnFailure;

    public SimData(int numAliens, float successRate, float avgStepsOnFailure) {
        this.numAliens = numAliens;
        this.successRate = successRate;
        this.avgStepsOnFailure = avgStepsOnFailure;
    }

    // deprecated
    public static void ExportToCSV(List<SimData> data) {
        var sb = new StringBuilder("Number of Aliens, Success Rate, Average Steps on Failure\n");
        foreach(SimData sim in data) {
            sb.Append(sim.numAliens + "," + sim.successRate + "," + sim.avgStepsOnFailure + "\n");
        }
        string filename = "simData.csv";
        int count = 1;
        while(System.IO.File.Exists(filename)) {
            filename = "simData" + count + ".csv";
            count++;
        }

        System.IO.File.WriteAllText(filename, sb.ToString());
    }
    
}

// writes simulation data to a csv file
class SimDataWriter {
    private string filename;

    public SimDataWriter(string botName = "") {
        var sb = new StringBuilder("Number of Aliens, Success Rate, Average Steps on Failure\n");
        string filename = botName + "simData.csv";
        int count = 1;
        while(System.IO.File.Exists(filename)) {
            filename = "simData" + count + ".csv";
            count++;
        }

        this.filename = filename;
        System.IO.File.WriteAllText(filename, sb.ToString());
    }

    public void Write(SimData sim) {
        var sb = new StringBuilder(sim.numAliens + "," + sim.successRate + "," + sim.avgStepsOnFailure + "\n");
        System.IO.File.AppendAllText(filename, sb.ToString());
    }

}

// c# doesn't have it's own list shuffle :/
static class ListExtensions {
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = ThreadSafeRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}



