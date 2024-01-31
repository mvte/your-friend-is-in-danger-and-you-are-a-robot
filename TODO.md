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