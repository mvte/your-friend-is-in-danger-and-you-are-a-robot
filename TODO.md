# Things I need to do
- Implement find_k
- Finish Results and Analysis
- Conclusions
    - Give credit to code from internet
- Get Bot 1 Data
- Comment code
- Fix Bot3
    - It's not using the buffer, need to rerun tests

## Bonus 1
### Method
We use a genetic algorithm to find the best ship configuration. 

We generate 100 ships (generation 1), and for each ship we run 100 simulations. Then, take the top 20 ships that perform the best. From these top 20, we recombine them to produce 100 more ships (generation 2). We save each generation in some database. We allow ourselves to load the generation in. 

We should show only one of 100 simulations running, and run the rest in the background. 

### Recombination
To combine two ships, we use uniform crossover. For each node in each ship, if both nodes are open, then the corresponding node in the new ship is open. If both are closed, the corresponding node is closed. If one is open and one is closed, we flip a weighted coin (based on success rates) to determine whether or not it is open or closed. 

Then, we ensure that the ship is valid, if not we try again.