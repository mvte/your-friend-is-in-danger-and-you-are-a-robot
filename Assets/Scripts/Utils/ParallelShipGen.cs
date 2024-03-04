namespace Utils {

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

// Generates a ship independent of any UnityEngine classes, so it can be used in a parallel context
public class ParallelShipGenerator {

    // generates ships of a given dimension in parallel
    public static ConcurrentBag<bool[,]> GenerateParallelShips(int dim, int shipsToGenerate = 100) {
        var ships = new ConcurrentBag<bool[,]>();
        Parallel.For(0, shipsToGenerate, (i) => {
            ships.Add(GenerateShip(dim));
        });
        return ships;
    }
    
    /**
    * Generates the structure of the ship by opening nodes as per the requirements
    * 1. Choose a square at random to open
    * 2. Iterate:
    *  a. Identify all currently blocked cells that have exactly one open neighbor
    *  b. Open one of these cells at random
    *  c. Repeat until no more cells can be opened
    * 3. Idenfity all cells that are 'dead ends' - open cells with one neighbor.
    * 4. For approximately half these cells, pick one of their closed neighbors at random and open it.
    */
    private static bool[,] GenerateShip(int dim) {
        bool[,] ship = new bool[dim, dim];
        int[,] dirs = new int[,]{{0, 1}, {0, -1}, {1, 0}, {-1, 0}}; 
        // choose a square at random to open
        int x = ThreadSafeRandom.Next(dim);
        int y = ThreadSafeRandom.Next(dim);
        ship[x, y] = true;

        
        List<(int, int)> openNodes = new List<(int, int)>{(x, y)};
        // iterate 
        for(;;) {
            // identify all blocked cells that have neighbors
            var candidates = new HashSet<(int, int)>();
            foreach(var node in openNodes) {
                for(int i = 0; i < 4; i++) {
                    int nx = node.Item1 + dirs[i, 0];
                    int ny = node.Item2 + dirs[i, 1];
                    if(nx >= 0 && nx < dim && ny >= 0 && ny < dim && !ship[nx, ny]) {
                        candidates.Add((nx, ny));
                    }
                }
            }

            // of those blocked cells, find the ones with exactly one open neighbor
            List<(int, int)> eligible = new List<(int, int)>();
            foreach(var candidate in candidates) {
                int count = 0;
                for(int i = 0; i < 4; i++) {
                    int nx = candidate.Item1 + dirs[i, 0];
                    int ny = candidate.Item2 + dirs[i, 1];
                    if(nx >= 0 && nx < dim && ny >= 0 && ny < dim && ship[nx, ny]) {
                        count++;
                    }
                }
                if(count == 1) {
                    eligible.Add(candidate);
                }
            }

            // if there are no eligible cells, we're done
            if(eligible.Count == 0) {
                break;
            }

            // open one of the eligible cells at random
            var chosen = eligible[ThreadSafeRandom.Next(eligible.Count)];
            ship[chosen.Item1, chosen.Item2] = true;
            openNodes.Add(chosen);
        }

        // find all dead ends - cells with one open neighbor
        List<(int, int)> deadEnds = new List<(int, int)>();
        foreach(var node in openNodes) {
            int count = 0;
            for(int i = 0; i < 4; i++) {
                int nx = node.Item1 + dirs[i, 0];
                int ny = node.Item2 + dirs[i, 1];
                if(nx >= 0 && nx < dim && ny >= 0 && ny < dim && ship[nx, ny]) {
                    count++;
                }
            }

            if(count == 1) {
                deadEnds.Add(node);
            }
        }

        // for approximately half the dead ends, open one of their closed neighbors at random
        foreach(var deadEnd in deadEnds) {
            if(ThreadSafeRandom.Next(2) == 0) {
                continue;
            }

            List<(int, int)> closedNeighbors = new List<(int, int)>();
            for(int i = 0; i < 4; i++) {
                int nx = deadEnd.Item1 + dirs[i, 0];
                int ny = deadEnd.Item2 + dirs[i, 1];
                if(nx >= 0 && nx < dim && ny >= 0 && ny < dim && !ship[nx, ny]) {
                    closedNeighbors.Add((nx, ny));
                }
            }

            if(closedNeighbors.Count > 0) {
                var chosen = closedNeighbors[ThreadSafeRandom.Next(closedNeighbors.Count)];
                ship[chosen.Item1, chosen.Item2] = true;
                openNodes.Add(chosen);
            }
        }

        return ship;
    }

    // generates a new generation of ships based off of the top ships from the previous generation
    public static ConcurrentBag<bool[,]> GeneticShipGeneration(List<Simulation> topShips, int numShips) {
        var ships = new ConcurrentBag<bool[,]>();
        // preserve top 3 of the best
        for (int i = 0; i < 3; i++) {
            ships.Add(topShips[i].booleanShip);
        }
        
        // make probability map for roulette selection
        List<float> probabilities = new List<float>();
        foreach (var ship in topShips) {
            probabilities.Add(ship.successes / (float)(ship.successes + ship.failures));
        }

        // make pairs with roulette selection
        List<(Simulation, Simulation)> pairs = new List<(Simulation, Simulation)>();
        for (int i = 0; i < numShips - 3; i++) {
            int firstSelect = RouletteSelection(probabilities);
            int secondSelect = RouletteSelection(probabilities);

            Simulation parent1 = topShips[firstSelect];
            Simulation parent2 = topShips[secondSelect];
            pairs.Add((parent1, parent2));
        }

        // generate children
        Parallel.ForEach(pairs, (pair) => {
            ships.Add(Crossover(pair.Item1, pair.Item2));
        });
        
        return ships;
    }

    public static float MUTATE_PROBABILITY = 0.05f;
    // performs uniform crossover on two ships
    private static bool[,] Crossover(Simulation sim1, Simulation sim2) {
        bool[,] ship1 = sim1.booleanShip;
        bool[,] ship2 = sim2.booleanShip;
        int dim = ship1.GetLength(0);
        // probability of a node being opened in the child (bias for open)
        float p = 0.65f;

        // if both nodes are the same, the child node is the same. 
        // otherwise, the child node is opened with probability p
        bool[,] child = new bool[dim, dim];
        for (int x = 0; x < dim; x++) {
            for (int y = 0; y < dim; y++) {
                if (ship1[x, y] == ship2[x, y]){
                    if(ThreadSafeRandom.NextFloat() < MUTATE_PROBABILITY) {
                        child[x, y] = !ship1[x, y];
                    } else {
                        child[x, y] = ship1[x, y];
                    }
                } else {
                    child[x, y] = ThreadSafeRandom.NextFloat() < p;
                }
            }
        }

        //verify child is valid (each open node is accessible from another open node)
        int numOpen = 0;
        for(int i = 0; i < dim; i++) {
            for(int j = 0; j < dim; j++) {
                if(child[i, j]) {
                    numOpen++;
                }
            }
        }

        // if not, we try again
        if(FloodFill(child) != numOpen) {
            return Crossover(sim1, sim2);
        }

        return child;
    }

    // returns the number of nodes that are accessible from the first open node
    private static int FloodFill(bool[,] ship) {
        int dim = ship.GetLength(0);
        bool[,] visited = new bool[dim, dim];
        int[,] dirs = new int[,]{{0, 1}, {0, -1}, {1, 0}, {-1, 0}};
        
        // find first open node
        int x = 0;
        int y = 0;
        for(int i = 0; i < dim; i++) {
            for(int j = 0; j < dim; j++) {
                if(ship[i, j]) {
                    x = i;
                    y = j;
                    break;
                }
            }
        }

        // perform flood fill
        int count = 0;
        var fringe = new Queue<(int, int)>();
        fringe.Enqueue((x, y));
        while(fringe.Count > 0) {
            var node = fringe.Dequeue();
            if(visited[node.Item1, node.Item2]) {
                continue;
            }
            visited[node.Item1, node.Item2] = true;
            count++;
            for(int i = 0; i < 4; i++) {
                int nx = node.Item1 + dirs[i, 0];
                int ny = node.Item2 + dirs[i, 1];
                if(nx >= 0 && nx < dim && ny >= 0 && ny < dim && ship[nx, ny] && !visited[nx, ny]) {
                    fringe.Enqueue((nx, ny));
                }
            }
        }

        return count;
    }

    // selects a ship based on the probabilities
    private static int RouletteSelection(List<float> probabilities) {
        float r = UnityEngine.Random.value;
        float sum = 0;
        for (int i = 0; i < probabilities.Count; i++) {
            sum += probabilities[i];
            if (r < sum) {
                return i;
            }
        }
        return probabilities.Count - 1;
    }

}

}