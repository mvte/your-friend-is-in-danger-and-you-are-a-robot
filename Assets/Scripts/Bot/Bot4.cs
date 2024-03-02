using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using Utils;

/**
* The idea behind this bot is to prioritize safety over making progress towards the captain. 
* At every time step, the bot chooses one of five actions:
* 1-4. Move in one of the four cardinal directions
* 5. Stay in place
* The bot chooses one of these actions depending on the following criteria:
* - The action is valid (i.e. the bot does not move onto an alien or into a closed node)
* - The action does not place the bot in a node adjacent to an alien
* - The action ideally is on the (buffered) path to the captain. If not, the bot chooses a safe action.
*/
public class Bot4 : Bot {

    public override string botName { get { return "Safe Pathing (Bot 4)";} }
    public List<Node> path;

    public override void computeNextStep(ShipManager ship) {
        // unhighlight the previous path
        if (path != null) {
            foreach(Node n in path) {
                n.Highlight();
            }
        }

        // determine the valid nodes the bot can move to (criterion 1)
        List<Node> validNodes = ship.GetValidNeighborNodes(pos);
        validNodes.Add(ship.GetNode(pos));

        // determine the nodes the bot can move to that are not adjacent to an alien (criterion 2)
        List<Node> safeNodes = new List<Node>();
        foreach(Node n in validNodes) {
            bool safe = true;
            foreach(Node neighbor in ship.GetNeighborNodes(n.pos)) {
                if(neighbor.occupied) {
                    safe = false;
                    break;
                }
            }
            if(safe) {
                safeNodes.Add(n);
            }
        }

        // if there are no safe nodes, we are in a bad spot and are most likely going to die soon
        if(safeNodes.Count == 0) {
            return;
        }

        // determine the ideal node to move to (criterion 3)
        path = a_star(ship.GetNode(pos), ship.GetNode(ship.captain.pos), ship);
        Node idealNode = path.Count != 0 ? path[0] : null;

        // choose the node to move to
        Node chosenNode;
        if (idealNode == null) {
            // if the ideal node does not exist, the bot chooses a random safe node (this is a point of optimization!)
            chosenNode = safeNodes[Random.Range(0, safeNodes.Count)];
        }
        else if(!safeNodes.Contains(idealNode)) {
            // if the ideal node exists but is not safe, the bot chooses the closest safe node to the ideal node
            safeNodes.Sort((n1, n2) => {
                float dist1 = Mathf.Abs(n1.pos.x - idealNode.pos.x) + Mathf.Abs(n1.pos.y - idealNode.pos.y);
                float dist2 = Mathf.Abs(n2.pos.x - idealNode.pos.x) + Mathf.Abs(n2.pos.y - idealNode.pos.y);
                return dist1.CompareTo(dist2); 
            }); 
            chosenNode = safeNodes[0];
        } else {
            // if the ideal node exists and is safe, the bot chooses the ideal node
            chosenNode = idealNode;
        }
        
        // if we are not following the path, then we unhighlight the path
        if(chosenNode != idealNode) {
            // unhighlight the path
            foreach(Node n in path) {
                n.Highlight();
            }
            //nullify the path so we don't highlight it again
            path = null;
        }

        // move to the chosen node
        pos = chosenNode.pos;
        transform.position = new Vector3(chosenNode.pos.x, chosenNode.pos.y, 0);
    }

    private List<Node> a_star(Node s, Node g, ShipManager ship) {
        // the set of nodes to be expanded
        var fringe = new PriorityQueue<Node, float>();
        // we start at s
        fringe.Enqueue(s, 0);
        // maps a node to its predecessor
        var prev = new Dictionary<Node, Node>();
        // we set the predecessor of s to be itself
        prev[s] = s;
        // maps a node to its distance from s
        var dist = new Dictionary<Node, float>();
        // the distance from s to s is 0
        dist[s] = 0;

        // while there are nodes to be expanded
        while(fringe.Count > 0) {
            // get the node with the smallest priority (distance + heuristic)
            Node curr = fringe.Dequeue();
            // if the goal has the smallest priority, then we are done
            if(curr.pos == g.pos) {
                break;
            }
            // get the neighbors of the current node
            List<Node> neighbors = ship.GetValidNeighborNodes(curr.pos);
            foreach(Node n in neighbors) {
                // for each neighbor of the current node's neighbors, we check if it is occupied
                foreach(Node nNeighbor in ship.GetNeighborNodes(n.pos)) {
                    // if the neighbor is occupied, then we skip it
                    if(nNeighbor.occupied) {
                        continue;
                    }
                }

                // compute the distance from s to n
                float tempDist = dist[curr] + 1.0f;
                // if the distance is less than the current distance to n, then we update the distance and the predecessor
                if (!dist.ContainsKey(n) || tempDist < dist[n]) {
                    dist[n] = tempDist;
                    prev[n] = curr;
                    fringe.Enqueue(n, tempDist + heuristic(n, g));
                }
            }
        }

        // there is no path to g
        if(!prev.ContainsKey(g)) {
            return new List<Node>();
        }

        // construct the path
        List<Node> path = new List<Node>();
        Node currentNode = g;
        while(currentNode.pos != s.pos) {
            path.Add(currentNode);
            currentNode = prev[currentNode];
        }
        path.Reverse();

        // highlight the path
        foreach(Node v in path) {
           v.Highlight();
        }

        return path;
    }

    // we use the manhattan distance as our heuristic
    private float heuristic(Node n, Node g) {
        return Mathf.Abs(n.pos.x - g.pos.x) + Mathf.Abs(n.pos.y - g.pos.y);
    }
}