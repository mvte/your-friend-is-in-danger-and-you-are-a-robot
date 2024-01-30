using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot1 : Bot
{
    List<Vector2> path;

    /**
    * This bot uses A* to find the shortest path to the captain
    */
    public override Vector2 computeNextStep(ShipManager ship) {
        if(path == null) {
            path = a_star(ship.captain, ship.aliens, ship.nodes);
        }
        if(path.Count == 0) {
            return pos;
        }

        Vector2 next = path[0];
        path.RemoveAt(0);
        return next;
    }

    private List<Vector2> a_star(Captain captain, List<Alien> aliens, Dictionary<Vector2, Node> nodes) {
        //TODO: implement this
        return new List<Vector2>();
    }
}
