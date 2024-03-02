using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{   
    // gives the alien position
    public Vector2 pos;
    
    // chooses the next step for the alien 
    public void computeNextStep(ShipManager ship) {
        //compute the valid neighbors
        List<Node> neighbors = ship.GetValidNeighborNodes(pos);
        // if there are no valid neighbors, don't do anything
        if(neighbors.Count == 0) {
            return;
        }

        // choose a random valid neighbor to move to 
        Node chosen = neighbors[Random.Range(0, neighbors.Count)];
        chosen.occupied = true;
        ship.GetNode(pos).occupied = false;
        pos = chosen.pos;
        transform.position = new Vector3(chosen.pos.x, chosen.pos.y, 0);
    }
}
