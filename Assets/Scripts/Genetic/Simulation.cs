using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using static ListExtensions;

public class Simulation 
{   
    public GeneticShip ship;
    public Bot bot;
    public bool[,] booleanShip;

    public const int MAX_STEPS = 1000;
    public int SimCount = 0;
    public int AlienCount = 0;

    // stats
    public int steps = 0;
    public int successes = 0;
    public int failures = 0;

    // status
    public bool finished = false;


    public Simulation(GeneticShip ship, bool[,] booleanShip, Bot bot, int simCount, int alienCount) {
        this.ship = ship;
        this.bot = bot;
        this.booleanShip = booleanShip;
        SimCount = simCount;
        AlienCount = alienCount;
    }

    public void Start() {
        ship.Init(bot, booleanShip, AlienCount);
        ship.Ready();
    }

    public void Step() {
        if(finished) {
            return;
        }

        if (steps == MAX_STEPS) {
            EndRun(false);
            return;
        }

        ship.bot.computeNextStep(ship, ship.front);
        
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
            alien.computeNextStep(ship, ship.front);
        }

        // check if the bot is on an alien
        if(botNode.occupied) {
            EndRun(false);
            return;
        }

        steps++;
    }

    public void EndRun(bool success) {
        // update success and failure counts
        if(success) {
            successes++;
        } else {
            failures++;
        }

        // decrement simulation count
        SimCount--;
        if(SimCount == 0) {
            EndSimulation();
            return;
        }

        // set up next run
        steps = 0;
        ship.Reset();
        ship.Init(bot, booleanShip, AlienCount);
        ship.Ready();
    }

    public void EndSimulation() {
        finished = true;
    }

    public (int, int) GetStats() {
        return (successes, failures);
    }

}