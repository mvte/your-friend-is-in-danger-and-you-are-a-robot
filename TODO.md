## Things I need to do
- Fix UI
- Fix Simulation Logic
- Add data recording
- Add end of simulation screen
- Maybe optimize ship construction algorithm. 

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
Run the algorithm asynchronously on a grid of booleans as opposed to on the actual nodes Dictionary. 
We then save the boolean grid, and reference it when we want to generate the the visual representation of the ship. 
We can save as many boolean grids in memory as we need (ideally, the same as the number of runs).
This approach wouldn't make as much of a difference on 
We can also potentially parallelize different parts of the algorithm. Such as:
- finding blocked nodes with open nieghbors
- finding blocked nodes with exactly one open neighbor
- finding dead ends

In all of these cases, we should use the ConcurrentBag class

Caching neighbors could also be of use. 
To maintain consistency, record the amount of time it takes to run 100 simulations
Also, keep note of <a href="https://stackoverflow.com/questions/19102966/parallel-foreach-vs-task-run-and-task-whenall">this<a>