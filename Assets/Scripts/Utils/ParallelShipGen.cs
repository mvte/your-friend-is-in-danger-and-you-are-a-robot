namespace Utils {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

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
    }
}