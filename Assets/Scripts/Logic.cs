using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Logic : MonoBehaviour
{
    public ShipManager ship;
    public Bot1 bot1Ref;
    public Bot2 bot2Ref;
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

    //config info
    private int configAlienCount = 32;
    private int configSimCount = 1;
    private int configBotSelection = 0;



    // Start is called before the first frame update
    void Start()
    {
        ship.Init(bot1Ref);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;

        bots = new List<Bot>{
            bot1Ref,
            bot2Ref,
        };
    } 

    //TODO: map to button
    public void RunSimulation(int dim = 32, int botSelection = 0, int alienCount = 32, int simCount = 1) {    
        Debug.Log("Simulation started");
        timeStart = Time.time;
        formManager.HideButtonsAndShowRunning();

        ship.Reset();
        ship.PregenerateShips(dim, simCount);
        ship.Init(bots[botSelection], dim, alienCount);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;
        ship.Ready();


        animate = simCount == 1;
        stepsOnFailure = new List<int>();
        steps = 0;
        successes = 0;
        failures = 0;
        runs = simCount;
        running = true;

        configSimCount = simCount;
        configAlienCount = alienCount;
        configBotSelection = botSelection;
    }

    public void EndRun(bool success) {
        running = false;

        if(success) {
            successes++;
        } else {
            failures++;
            stepsOnFailure.Add(steps);
        }

        steps = 0;
        runs--;
        if(runs == 0) {
            EndSimulation();
            return;
        }

        ship.Reset();
        ship.Init(bot1Ref);
        ship.Ready();

        running = true;
    }

    public void EndSimulation() {
        float avgStepsOnFailure = 0;
        foreach(int steps in stepsOnFailure) {
            avgStepsOnFailure += steps;
        }
        avgStepsOnFailure /= stepsOnFailure.Count;
        string botName = bots[configBotSelection].botName;

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
        if(!running) {
            return;
        }
        if(steps >= MAX_STEPS) {
            Debug.Log("Max steps reached");
            EndRun(false);
            return;
        }
        if(animate) {
            accumulatedTime += Time.deltaTime;
            if(accumulatedTime < 0.2f) {
                return;
            }
        }
        accumulatedTime = 0;

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

// c# doesn't have it's own list shuffle :/
static class ListExtensions {
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}


