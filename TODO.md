## Things I need to do
- Create routine that continously runs simulations, incrementing the number of aliens after some amount of simulations.
This routine should end when we reach a number of aliens where no simulations are successful
- Implement Bot 3 properly, not sure that the heuristic is good enough
    - Maybe, when we consider a node, we only add it to the priority queue if none of its neighbors are occupied by aliens.

### Simulation Logic
1. Set runs to be the amount of simulations we run
2. Begin the run
3. When the run ends, we call the End() method.
4. If we're done running simulations, we call the EndSimulation() method. 

#### End Method
The end method has the following responsibilities
- Records run-specific data (total steps, success count, etc.)
    - This means the method should take a parameter
- Resets all run-specific variables (steps)
- Decrements the run counter
- Calls EndSimulation() when we have ran the total number of simulations as desired and return
- Otherwise, run the next simulation.

#### End Simulation Method
This method aggregates the results of all of the runs, and displays them neatly. 

### Potential GenerateShip Speed Up
We can potentially speed up GenerateShip by decoupling its logic from the UnityEngine library. 
First, Run the algorithm asynchronously on a grid of booleans as opposed to on the actual nodes Dictionary. 
Then save the boolean grid, and reference it when we want to generate the the visual representation of the ship. 
We can save as many boolean grids in memory as we need (ideally, the same as the number of runs).
Further optimizations can be found can in parallelizing different parts of the algorithm. Such as:
- finding blocked nodes with open nieghbors
- finding blocked nodes with exactly one open neighbor
- finding dead ends

In all of these cases, we should use the ConcurrentBag class

To maintain consistency, record the amount of time it takes to run 100 simulations using a 32x32 grid and 32 aliens.


#### GenerateShip stats
Change | Time 
---|---
No Optimization | 37.29915s
Parallel Processing of Nodes | 42.05054s
Pregenerate ship layouts | 10.85372s
Pregenerated in built env | 4.79s (!!!)


#### Notes
- Parallel processing of nodes is slower! Probably because of the overhead of the Parallel.foreach function itself. 
- holy smokes pregeneration is cracked
- neighbor caching?