# CS520 - Project 1
by Jan Marzan

## Abstract
This project explores pathfinding and decision-making strategies for an autonomous agent operating within a dynamic and hostile environment. In a simulated space salvage vessel overrun by aliens, a robotic cleaning unit must rescue the ship's captain while avoiding detection. Four bot strategies were implemented and evaluated: a baseline shortest-path algorithm, variations incorporating dynamic path replanning and alien avoidance, and a custom strategy that prioritizes safety over progress. Success rates and survivability were analyzed under varying alien densities, providing insights into the efficacy of different approaches to navigation and decision-making in uncertain, adversarial settings. 

The rest of the report can be found in `/Report` as `Jan Marzan - Project 1 Report`. 

## Project Logistics
All relevant code can be found in `/Assets/Scripts/`, and each file's purpose explained in the report.

## Running
The Windows (64-bit) build is located in the `Windows/` folder and is named `your friend is in danger and you are a robot.exe`. The Mac build (Apple Silicon) is located in the `Mac/` folder and is named `yfiidayaabb`. If you would like to open the project in the Unity Editor, select the `your friend is in danger and you a robot/` directory as the root for thr project in Unity Hub and open from there. Please note that the game builds are very buggy and have high tendencies to crash. 