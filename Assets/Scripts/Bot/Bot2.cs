using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;
using System.Threading.Tasks;

/**
* At every time step, the bot re-plans the shortest path to the Captain, avoiding the current alien
* positions, and then executes the next step in that plan. Because it repeatedly replans, it constantly adapts to
* the movement of the aliens.
*/
public class Bot2 : Bot {
    public List<Node> path;

    public override string botName { get { return "Adaptive Path (Bot 2)";} }
    public override void computeNextStep(ShipManager ship, bool front = true) {
        // we highlight the path
        if (path != null) {
            foreach(Node n in path) {
                n.Highlight();
            }
        }
        // if the bot is at the captain's position, then we are done
        if(this.pos == ship.captain.pos) {
            return;
        }

        // at every time step, we compute a new path to the captain
        Node startNode = ship.GetNode(this.pos);
        Node captainNode = ship.GetNode(ship.captain.pos);
        path = a_star(startNode, captainNode, ship.aliens, ship.nodes, ship);
        // if there is no path, we stay put
        if(path.Count == 0) {
            return;
        }

        // move to the next node in the path
        Vector2 next = path[0].pos;
        path[0].Highlight();
        path.RemoveAt(0);

        this.pos = next;
        this.transform.position = new Vector3(next.x, next.y, 0);
    }
    

    private List<Node> a_star(Node s, Node g, List<Alien> aliens, Dictionary<Vector2, Node> nodes, ShipManager ship) {
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
                // compute the distance from s to n
                float tempDist = dist[curr] + 1.0f;
                // if the distance is less than the current distance to n, then we update the distance and the predecessor
                if (!dist.ContainsKey(n) || tempDist < dist[n]) {
                    dist[n] = tempDist;
                    prev[n] = curr;
                    // we add n to the fringe with new priority distance + heuristic
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