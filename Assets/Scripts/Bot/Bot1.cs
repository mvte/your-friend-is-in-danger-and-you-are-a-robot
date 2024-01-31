using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;

public class Bot1 : Bot
{
    List<Node> path;

    /**
    * This bot uses A* to find the shortest path to the captain
    */
    public override void computeNextStep(ShipManager ship) {
        if(path == null) {
            Node startNode = ship.GetNode(this.pos);
            Node captainNode = ship.GetNode(ship.captain.pos);
            path = a_star(startNode, captainNode, ship.aliens, ship.nodes, ship);
        }
        if(path.Count == 0) {
            return;
        }

        Vector2 next = path[0].pos;
        path[0].Highlight();
        path.RemoveAt(0);

        this.pos = next;
        this.transform.position = new Vector3(next.x, next.y, 0);
    }

    private List<Node> a_star(Node s, Node g, List<Alien> aliens, Dictionary<Vector2, Node> nodes, ShipManager ship) {
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
