using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;
using System.Threading.Tasks;

/**
* At every time step, the bot re-plans the shortest path to the Captain, avoiding the current alien
* positions and any cells adjacent to current alien positions, if possible, then executes the next step in that plan.
* If there is no such path, it plans the shortest path based only on current alien positions, then executes the next
* step in that plan. Note: Bot 3 resorts to Bot 2 behavior in this case
*/
public class Bot3 : Bot {
    public List<Node> path;
    private ShipManager ship;


    public override string botName { get { return "Buffered Adaptive Path (Bot 3)";} }
    public override void computeNextStep(ShipManager ship) {
        if(this.ship == null) {
            this.ship = ship;
        }
        if (path != null) {
            foreach(Node n in path) {
                n.Highlight();
            }
        }
        if(this.pos == ship.captain.pos) {
            return;
        }
        Node startNode = ship.GetNode(this.pos);
        Node captainNode = ship.GetNode(ship.captain.pos);
        path = a_star(startNode, captainNode, ship.aliens, ship.nodes, ship);
        if(path.Count == 0) {
            return;
        }

        Vector2 next = path[0].pos;
        path[0].Highlight();
        path.RemoveAt(0);

        this.pos = next;
        this.transform.position = new Vector3(next.x, next.y, 0);
    }
    

    private List<Node> a_star(Node s, Node g, List<Alien> aliens, Dictionary<Vector2, Node> nodes, ShipManager ship, bool useBuffer = true) {
        var fringe = new PriorityQueue<Node, float>();
        fringe.Enqueue(s, 0);
        var prev = new Dictionary<Node, Node>();
        prev[s] = s;
        var dist = new Dictionary<Node, float>();
        dist[s] = 0;

        while(fringe.Count > 0) {
            Node curr = fringe.Dequeue();
            if(curr.pos == g.pos) {
                break;
            }
            List<Node> neighbors = ship.GetValidNeighborNodes(curr.pos);
            foreach(Node n in neighbors) {
                foreach(Node nNeighbor in ship.GetNeighborNodes(n.pos)) {
                    if(nNeighbor.occupied) {
                        continue;
                    }
                }

                float tempDist = dist[curr] + 1.0f;
                if (!dist.ContainsKey(n) || tempDist < dist[n]) {
                    dist[n] = tempDist;
                    prev[n] = curr;
                    fringe.Enqueue(n, tempDist + heuristic(n, g));
                }
            }
        }

        // there is no path to g
        if(!prev.ContainsKey(g)) {
            // we recompute the path without the buffer (bot2 behavior)
            if(useBuffer) return a_star(s, g, aliens, nodes, ship, false);
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