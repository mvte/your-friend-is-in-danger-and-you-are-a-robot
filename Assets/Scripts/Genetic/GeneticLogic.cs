using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Profiling;
using Utils;

public class GeneticManager : MonoBehaviour {
    // state
    public State state { get; private set; }

    // references
    public Simulation[] Simulations;
    public GameObject cam;
    public GameObject GeneticShipRef;
    public Bot bot;
    private GenDataWriter genDataWriter;

    // config variables
    [SerializeField]
    int Generation = 0;
    int MaxGenerations = 50;
    int ShipCount = 50;
    int SimCount = 25;
    int Dim = 32;
    int AlienCount = 64;

    void Start() => ChangeState(State.Initializing);

    public void ChangeState(State newState) {
        state = newState;
    }

    void Update() {
        switch (state) {
            case State.Initializing:
                HandleInitializing();
                break;
            case State.Ready:
                break;
            case State.StartingSimulations:
                HandleStartingSimulations();
                break;
            case State.Simulating:
                HandleSimulating();
                break;
            case State.GenerationTransition:
                HandleGenerationTransition();
                break;
            case State.Done:
                HandleDone();
                break;
        }
    }


    // place camera in correct position
    void HandleInitializing() {
        Debug.Log("Initializing");
        Application.runInBackground = true;

        // center camera
        cam.transform.position = new Vector3(Dim/2 - 0.5f, Dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = Dim/2 + 1;

        genDataWriter = new GenDataWriter();

        ChangeState(State.Ready);
    }

    // generates first generation ships, and constructs simulations in the array
    void HandleStartingSimulations() {
        Debug.Log("Starting simulations");
        Simulations = new Simulation[ShipCount];
        ConcurrentBag<bool[,]> pregeneratedShips = ParallelShipGenerator.GenerateParallelShips(Dim, ShipCount);

        for (int i = 0; i < ShipCount; i++) {
            pregeneratedShips.TryTake(out bool[,] ship);
            GeneticShip spawnedGeneticShip = Instantiate(GeneticShipRef, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<GeneticShip>();
            spawnedGeneticShip.transform.parent = transform;
            Simulations[i] = new Simulation(spawnedGeneticShip, ship, bot, SimCount, AlienCount);
            Simulations[i].Start();
        }

        //bring one ship to the front
        Simulations[0].ship.BringToFront();

        ChangeState(State.Simulating);
    }

    // runs the simulation for all ships, and saves data on finish
    void HandleSimulating() {
        bool finished = true;

        for (int i = 0; i < ShipCount; i++) {
            Simulations[i].Step();
            finished = finished && Simulations[i].finished;
        }

        if(finished) {
            ChangeState(State.GenerationTransition);
        }
    }

    // generates the next generation of ships, constructs simulations in the array
    void HandleGenerationTransition() {
        Debug.Log("Generation transition");
        Generation++;

        // write data to file
        int successes = 0;
        int failures = 0;
        for(int i = 0; i < ShipCount; i++) {
            successes += Simulations[i].successes;
            failures += Simulations[i].failures;
        }
        genDataWriter.Write(Generation, successes, failures);

        // sort simulations by success
        Array.Sort(Simulations, (a, b) => a.successes.CompareTo(b.successes));

        // store top 20% of ships to create next generation
        List<Simulation> topShips = new List<Simulation>();
        for (int i = 0; i < ShipCount/5; i++) {
            topShips.Add(Simulations[i]);
        }

        // if we are at the last generation, we are done
        if (Generation == MaxGenerations) {
            ChangeState(State.Done);
            return;
        }

        // destroy all current simulations
        for (int i = 0; i < ShipCount; i++) {
            Destroy(Simulations[i].ship.gameObject);
        }
        Simulations = new Simulation[ShipCount];

        // generate new ships
        ConcurrentBag<bool[,]> nextGen = ParallelShipGenerator.GeneticShipGeneration(topShips, ShipCount);
        for (int i = 0; i < ShipCount; i++) {
            nextGen.TryTake(out bool[,] ship);
            GeneticShip spawnedGeneticShip = Instantiate(GeneticShipRef, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<GeneticShip>();
            spawnedGeneticShip.transform.parent = transform;
            Simulations[i] = new Simulation(spawnedGeneticShip, ship, bot, SimCount, AlienCount);
            Simulations[i].Start();
        }
        Simulations[0].ship.BringToFront();
        ChangeState(State.Simulating);
    }

    // showcase best performing ship
    void HandleDone() {
        Simulations[0].ship.Showcase();
        genDataWriter.Write(Generation, Simulations[0].successes, Simulations[0].failures);

        Debug.Log("Best Success Rate: " + (float)Simulations[0].successes/(Simulations[0].successes+Simulations[0].failures)); 
        ChangeState(State.Ready);       
    }

    // starts the simulation
    // TODO: add a button that calls this 
    [ContextMenu("Start Simulations")]
    public void StartSimulations() {
        if(state == State.Ready) {
            ChangeState(State.StartingSimulations);
        }
    }
}

// enum for the state of the genetic manager
public enum State {
    Initializing,
    Ready,
    StartingSimulations,
    Simulating,
    GenerationTransition,
    Done 
}

public class GenDataWriter {
    private string filename;

    public GenDataWriter(string filename = "genData.csv") {
        string headers = "Generation,Successes,Failures,Success Rate\n";
        int count = 1;
        while (System.IO.File.Exists(filename)) {
            filename = $"genData{count}.csv";
            count++;
        }

        this.filename = filename;
        File.WriteAllText(filename, headers);
    }

    public void Write(int generation, int successes, int failures) {
        string data = $"{generation},{successes},{failures},{(float)successes/(successes+failures)}\n";
        File.AppendAllText(filename, data);
    }
}

