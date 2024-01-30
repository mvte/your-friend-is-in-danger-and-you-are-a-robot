using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Logic : MonoBehaviour
{
    public ShipManager ship;
    public Bot1 bot1Ref;
    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        ship.Init(bot1Ref);
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;

        RunSimulation();
    }

    public void RunSimulation(int runs = 1, bool animate = false) {    
        for(int i = 0; i < runs; i++) {
            Debug.Log($"Simulation {i + 1} started");
            ship.Reset();
            ship.Ready();

            // advance the bot
            ship.bot.computeNextStep(ship);

            // determine if the bot is on an alien
            foreach(Alien alien in ship.aliens) {
                if(ship.bot.pos == alien.pos) {
                    Debug.Log("Bot hit alien");
                    continue;
                }
            }
            
            // determine if the bot is on the captain
            if(ship.bot.pos == ship.captain.pos) {
                Debug.Log("Bot reached the captain");
                continue;
            }

            // advance aliens
            List<Alien> randomizedAliens = new List<Alien>(ship.aliens);
            randomizedAliens.Shuffle();
            foreach(Alien alien in randomizedAliens) {
                alien.computeNextStep(ship);
            }

            // check if the bot is on an alien
            foreach(Alien alien in ship.aliens) {
                if(ship.bot.pos == alien.pos) {
                    Debug.Log("Bot hit alien");
                    continue;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
