using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Logic : MonoBehaviour
{
    public ShipManager ship;
    public Bot1 bot1Ref;
    public GameObject cam;
    public bool running;
    public int steps;
    public int runs = 1;

    public bool animate = false;
    private float accumulatedTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        ship.Init(bot1Ref);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;

        // only for now
        RunSimulation();
    } 

    //TODO: map to button
    public void RunSimulation() {    
        Debug.Log("Simulation started");
        running = true;
        ship.Reset();
        ship.Ready();
    }

    // Update is called once per frame
    void Update()
    {   
        if(!running) {
            return;
        }
        if(steps > 1000) {
            steps = 0;
            Debug.Log("simulation end");
            runs--;
            return;
        }
        if(runs == 0) {
            running = false;
            return;
        }

        if(animate) {
            accumulatedTime += Time.deltaTime;
            if(accumulatedTime < 0.2f) {
                return;
            }
        }
        accumulatedTime = 0;

        // advance the bot
        ship.bot.computeNextStep(ship);

        // determine if the bot is on an alien
        Node botNode = ship.GetNode(ship.bot.pos);
        if(botNode.occupied) {
            Debug.Log("Bot hit alien");
            steps = 1001;
            return;
        }
        
        // determine if the bot is on the captain
        if(ship.bot.pos == ship.captain.pos) {
            Debug.Log("Bot reached the captain");
            steps = 1001;
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
            Debug.Log("Bot hit alien");
            steps = 1001;
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


